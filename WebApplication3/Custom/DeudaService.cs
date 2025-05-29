using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication3.Controllers;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Custom
{
    public class DeudorService
    {
        private readonly LumitecContext _context;

        public DeudorService(LumitecContext context)
        {
            _context = context;
        }

        public async Task<List<DeudaResumenDTO>> ObtenerResumenDeDeudasPorIdConsumidorAsync(int idConsumidor)
        {
            var deudaResumen = new List<DeudaResumenDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("ObtenerResumenDeDeudasPorIdConsumidor", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@IdConsumidor", idConsumidor);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                deudaResumen.Add(new DeudaResumenDTO
                                {
                                    NombreConsumidor = reader["NombreConsumidor"].ToString(),
                                    Deuda = reader.GetDecimal(reader.GetOrdinal("Deuda")),
                                    FechaVenta = reader.GetDateTime(reader.GetOrdinal("FechaVenta"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones centralizado (log, rethrow, etc.)
                throw new Exception("Error al obtener el resumen de deudas", ex);
            }

            return deudaResumen;
        }

        public async Task<List<DetalleDeudaDTO>> ObtenerDetallesDeDeudaAsync(int idListado)
        {
            var detallesDeuda = new List<DetalleDeudaDTO>();

            try
            {
                // Consulta LINQ optimizada
                detallesDeuda = await Task.Run(() =>
                    _context.DetalleListaNegras
                        .Where(d => d.IdListado == idListado)
                        .Join(
                            _context.Products,
                            detalle => detalle.IdProducto,
                            producto => producto.IdProduct,
                            (detalle, producto) => new DetalleDeudaDTO
                            {
                                Producto = producto.ProductName,
                                Cantidad = detalle.Cantidad,
                                Precio = detalle.Precio,
                                Total = detalle.Cantidad * detalle.Precio
                            })
                        .ToList());
            }
            catch (Exception ex)
            {
                // Manejo de excepciones centralizado
                throw new Exception("Error al obtener detalles de la deuda", ex);
            }

            return detallesDeuda;


        }
    }

}
