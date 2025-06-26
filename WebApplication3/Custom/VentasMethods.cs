using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Custom
{
    public class VentasMethods
    {
        private readonly LumitecContext _context;
        private readonly ILogger<ClsListaNegraMetodos> _logger;
        public VentasMethods(LumitecContext context, ILogger<ClsListaNegraMetodos> logger)
        {
            _context = context;
            _logger = logger;
        }



        // Actualizar la función GenerarRecibo para aceptar un MemoryStream
        public static void GenerarRecibo(string nombreCliente, DateTime fecha,
                                       List<Producto> productos, MemoryStream memoryStream)
        {
            try
            {
                // Crear el documento PDF
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                // Cargar la fuente estándar
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Agregar título y encabezado
                Paragraph titulo = new Paragraph("RECIBO")
                    .SetFont(font)
                    .SetFontSize(14)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(titulo);

                Paragraph cliente = new Paragraph($"Nombre del cliente: {nombreCliente ?? "Sin nombre"}")
                    .SetFont(font)
                    .SetFontSize(10);
                document.Add(cliente);

                Paragraph direccion = new Paragraph("Asunción Mita, Jutiapa")
                    .SetFont(font)
                    .SetFontSize(10);
                document.Add(direccion);

                Paragraph fechaTexto = new Paragraph($"Fecha: {fecha.ToString("dd/MM/yyyy")}")
                    .SetFont(font)
                    .SetFontSize(10);
                document.Add(fechaTexto);

                // Crear tabla para los productos
                Table table = new Table(new float[] { 1, 3, 2, 2 }).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Cantidad").SetFont(font).SetFontSize(10)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(font).SetFontSize(10)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Precio Unitario").SetFont(font).SetFontSize(10)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Importe").SetFont(font).SetFontSize(10)));

                // Iterar sobre los productos y llenar la tabla
                foreach (var producto in productos)
                {
                    string descripcion = producto.Descripcion ?? "Producto sin descripción";
                    int cantidad = producto.Cantidad;
                    decimal precioUnitario = producto.PrecioUnitario;
                    decimal importe = cantidad * precioUnitario;

                    table.AddCell(new Cell().Add(new Paragraph(cantidad.ToString()).SetFont(font).SetFontSize(10)));
                    table.AddCell(new Cell().Add(new Paragraph(descripcion).SetFont(font).SetFontSize(10)));
                    table.AddCell(new Cell().Add(new Paragraph(precioUnitario.ToString("F2")).SetFont(font).SetFontSize(10)).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(importe.ToString("F2")).SetFont(font).SetFontSize(10)).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                }

                // Agregar la tabla al documento
                document.Add(table);

                // Calcular y agregar el total
                decimal total = productos.Sum(p => p.Cantidad * p.PrecioUnitario);
                Paragraph totalTexto = new Paragraph($"Total: {total.ToString("F2")}")
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                document.Add(totalTexto);

                // Cerrar el documento
                document.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el recibo: {ex.Message}");
                throw;
            }
        }


        // Clase para representar los productos
        public class Producto
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Importe => Cantidad * PrecioUnitario;
        }

        public async Task Vendido(List<VentasDTO> detalles, decimal deudaTotal)
        {
            if (detalles == null || !detalles.Any())
            {
                throw new ArgumentException("No se pueden insertar registros vacíos.");
            }

            // Validar que todos los detalles sean válidos
            foreach (var detalle in detalles)
            {
                var context = new ValidationContext(detalle);
                Validator.ValidateObject(detalle, context, validateAllProperties: true);
            }

            // Crear DataTable para enviar los detalles
            var detallesTable = new DataTable();
            detallesTable.Columns.Add("IdProducto", typeof(int));
            detallesTable.Columns.Add("Cantidad", typeof(int));
            detallesTable.Columns.Add("Precio", typeof(decimal));

            foreach (var detalle in detalles)
            {
                detallesTable.Rows.Add(detalle.IdProducto, detalle.Cantidad, detalle.Precio);
            }

            // Conexión a la base de datos
            using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                await connection.OpenAsync();

                // Iniciar transacción
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Configuración del comando
                        using (var command = new SqlCommand("dbo.InsertarVentaConDetallesConTransaccion", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            // Agregar parámetros
                            command.Parameters.AddWithValue("@IdUsuario", detalles[0].IdConsumidor);
                            command.Parameters.AddWithValue("@Id_Consumidor", detalles[0].IdElectricista);
                            command.Parameters.AddWithValue("@Total", deudaTotal);
                            command.Parameters.AddWithValue("@DetalleVenta", detallesTable).SqlDbType = SqlDbType.Structured;

                            // Ejecutar el procedimiento
                            await command.ExecuteNonQueryAsync();
                        }

                        // Confirmar la transacción
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                        // Revertir la transacción en caso de error
                        transaction.Rollback();
                        throw new Exception("Error al insertar la venta en la base de datos.", ex);
                    }
                }
            }
        }

    }
}
