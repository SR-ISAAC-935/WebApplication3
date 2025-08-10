var registros = {};
let facturacion = {}
var facxm = "";
var tempcf ="";
document.addEventListener('DOMContentLoaded', function () {
    const checkboxPrecio = document.getElementById('validarPrecio');
    const precioInput = document.getElementById('Precio');

    checkboxPrecio.addEventListener('change', function () {
        if (this.checked) {
            precioInput.removeAttribute('readonly');
        } else {
            precioInput.setAttribute('readonly', 'readonly');
        }
    });

    const checkboxNegro = document.getElementById('validarListaNegra');
    const buttonNegro = document.getElementById('ListaNegra');

    checkboxNegro.addEventListener('change', function () {
        if (this.checked) {
            buttonNegro.style.display = 'inline-block'; // Mostrar el botón
        } else {
            buttonNegro.style.display = 'none'; // Ocultar el botón
        }
    });
});


$(document).ready(function () {
    let timer; // Usamos un timer para evitar hacer demasiadas peticiones

    $('#Nit').on('input', function () {
        clearTimeout(timer); // Reinicia el timer si el usuario sigue escribiendo

        const nitValue = $(this).val().trim();

        if (nitValue.length > 6) {
            // Espera 500ms después del último input antes de hacer la petición
          

            timer = setTimeout(function () {
                $.ajax({
                    url: '/Facturas/ConsultaNit',
                    method: 'GET',
                    data: { nit: nitValue },
                    success: function (response) {
                        if (Array.isArray(response) && response.length > 0) {
                            const info = response[0];
                          
                            facturacion = response.map(reg => ({
                                id: reg.id,
                                tax_code_type: reg.tax_code_type,
                                tax_code: reg.tax_code,
                                tax_name: reg.tax_name,
                                name: reg.name,
                            }))
                           // console.log("jaja yolo",Facturacion)
                            $("#info").text(info.tax_name).show();
                        } else {
                            $("#info").text(`No se encontró información para este NIT.`).show();
                        }
                    },
                    error: function (xhr) {
                        let message = "Ocurrió un error desconocido.";
                        try {
                            const errorResponse = JSON.parse(xhr.responseText);
                            message = errorResponse.error || message;
                        } catch (e) {
                            console.error("No se pudo parsear el error como JSON:", e);
                        }
                        $("#info").text(`No se encontró información para este NIT. ${message}`).show();
                    }
                });
            }, 500); // Espera medio segundo desde el último input
        } else {
            $("#info").hide(); // Oculta el mensaje si no hay suficientes caracteres
        }
    });
});
$('form').on('submit', function (e) {
    e.preventDefault();

    const registrosArray = Object.values(registros).filter(r => r.cantidad >= 1);
    if (registrosArray.length === 0) {
        alert("Todos los productos tienen cantidad menor a 1. Verifica los datos.");
        return;
    }
    const consumidor = facturacion[0]; // asumimos solo uno para la factura
   // console.log(consumidor.tax_code,"tax code")
    const fechaVenta = new Date().toISOString();
    //console.log(registros,"registros")
    const cfnit = (
        !Array.isArray(facturacion) || !facturacion[0]?.tax_code || facturacion[0].tax_code === "0"
    ) ? tempcf : facturacion[0].tax_code
    const cfconsumidor = (
        !Array.isArray(facturacion) || !facturacion[0]?.idConsumidor || facturacion[0].idConsumidor === null
    ) ? 1 : facturacion[0].tax_code
    const cfconsumer = (
        !Array.isArray(facturacion) || !facturacion[0]?.tax_name || facturacion[0].tax_name === ""
    ) ? "" : facturacion[0].tax_name
    const ventas = registrosArray.map(reg => ({
        direccion: $('#Direccion').val(),
        nit: cfnit,
        IdConsumidor: cfconsumidor,
        Deuda: reg.deuda,
        FechaVenta: fechaVenta,
        IdProducto: reg.idProducto,
        ProductName: reg.producto,
        Cantidad: reg.cantidad,
        Precio: reg.precio,
        tax_name: cfconsumer
    }));
    console.log("nieva facturacion ", facturacion)
    const consumidoresJson = JSON.stringify(ventas);
    const deudaTotal = ventas.reduce((acc, reg) => acc + reg.Deuda, 0);

    console.log("JSON a enviar:", ventas);
    console.log("Deuda Total:", deudaTotal);
    console.log("json consumidor ", consumidoresJson)

    $.ajax({
        url: $(this).attr('action'),
        method: $(this).attr('method'),
        contentType: 'application/json',
        data: JSON.stringify({ consumidoresJson, deudaTotal }),
        success: function (Datos) {
            console.log('verificacion de mensajes',Datos)
            console.log(Datos.mensaje);
            console.log(Datos.pdf)
            console.log(Datos.xml)
            

            if (Datos && Datos.xml) {
                const link = document.getElementById('FEL');
                link.style.display = 'inline-block';
                link.textContent = 'Ver FEL';

                const af = `/Facturas/DescargarRecibo?xmlurl=${encodeURIComponent(Datos.xml)}`;

                fetch(af)
                    .then(response => {
                        if (!response.ok) throw new Error('No se pudo obtener el PDF');
                        return response.blob();
                    })
                    .then(blob => {
                        const blobUrl = window.URL.createObjectURL(blob);
                        const newTab = window.open(blobUrl, '_blank');
                        if (newTab) {
                            newTab.focus();
                        } else {
                            alert('Por favor, habilita los pop-ups para esta página.');
                        }
                    })
                    .catch(error => {
                        console.error('Error al abrir el PDF:', error);
                        alert('Ocurrió un error al mostrar la factura.');
                    });
            } else {
                const link = document.getElementById('FEL');
                link.textContent = 'No hay FEL disponible';
                link.removeAttribute('href');
                link.removeAttribute('target');
                link.style.display = 'inline-block';
            }


        }
,
        error: function () {
            alert('Error al realizar la venta');
        }
    });
});
