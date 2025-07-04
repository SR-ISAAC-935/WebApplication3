﻿function showError(message) {
    alert(message);
   
}
let consumidor = "";
let electrcista = "";
let detallesAbonados = [];
let detallesAbono = [];
$("#ConsumidorInput").on("input", function () {
    const term = $(this).val();
    if (term.length < 3) {
        $("#ConsumidorList").hide();
        return;
    }

    $.get(`/Deudor/BuscarConsumidor?term=${encodeURIComponent(term)}`)
        .done(function (data) {
            const list = $("#ConsumidorList");
            list.empty();
            if (data && data.length > 0) {
                data.forEach(c => {
                    list.append(`<li class="list-group-item" data-id="${c.idConsumidor}">${c.nombreConsumidor}</li>`);
                });
                list.show();
            } else {
                list.hide();
            }
        })
        .fail(() => showError("Error al buscar consumidores."));
});

$("#ConsumidorList").on("click", ".list-group-item", function () {
    const id = $(this).data("id");
    $("#ConsumidorInput").val($(this).text());
    $("#IdConsumidor").val(id);
    $("#ConsumidorList").hide();
});

$("#btnObtenerFacturas").on("click", function () {
    const idConsumidor = $("#IdConsumidor").val();
    const validationMessage = $("#IdConsumidorValidation");

    if (!idConsumidor) {
        validationMessage.text("Por favor, seleccione un consumidor válido.");
        return;
    }
    validationMessage.text("");

    const button = $(this).prop("disabled", true).text("Cargando...");

    $.ajax({
        url: '/Deudor/ObtenerDeudasPorConsumidor',
        type: "GET",
        data: { idConsumidor },
    })
        .done(function (data) {
            const tbody = $("#DeudasTableBody");
            tbody.empty();

            if (!data || data.length === 0) {
                alert("No se encontraron deudas para este consumidor.");
                $("#DeudasContainer").hide();
                return;
            }

            data.forEach(d => {
                electrcista = d.electrcista;
                consumidor = d.consumidor;
                tbody.append(`
                        <tr>
                         <td>${d.deuda.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                            <td>${d.electricista}</td>
                            <td>${d.consumidor}</td>
                            <td>${new Date(d.fechaVenta).toLocaleDateString()}</td>
                            <td><button class="btn btn-primary btn-detalles" data-id="${d.idListado}">Ver detalles</button></td>
                             <td><input type="text" name="AbonoDeuda" class="AbonoDeuda" placeholder="Ingrese abono a deuda" /></td>
                     <td><button class="btn btn-danger btn-Cancelar" data-id="${d.idListado}">Abonar Deuda</button></td>
                      <td><button class="btn btn-primary btn-AbonoDetalles" data-id="${d.idListado}">Detalles Abonos</button></td>
                        </tr>
                    `);
            });
            actualizarTotalEnTabla();
            $('#DeudasPendientes').hide();
            $("#DeudasContainer").show();
        })
        .fail(() => showError("Error al obtener las deudas."))
        .always(() => button.prop("disabled", false).text("Obtener Facturas"));
});

$("#DeudasTableBody").on("click", ".btn-Cancelar", function () {
    const fila = $(this).closest("tr");
    const abono = fila.find('input[name="AbonoDeuda"]').val();
    const idListado = $(this).data("id");

    if (!idListado) {
        showError("ID de deuda no válido.");
        return;
    }
    $.ajax({
        url: '/Deudor/actualizarDeuda',
        type: "PUT",
        data: { idListado, abono },
    }).done(function (data) {
        console.log(data)
        location.reload();
    }).fail(function (xhr) {
        alert(`hubo un error en ${xhr.responseText}`);
    });

});
$('#AbonosTableBody tr').each(function () {
    var fila = $(this);
    var filaData = [];

    fila.find('<td>').each(function () {
        filaData.push($(this).text().trim());
    });
    detallesAbonados.push(filaData);
});

$("#DeudasTableBody").on("click", ".btn-Cancelar", function () {
    const fila = $(this).closest("tr");
    const abonoo = fila.find('input[name="AbonoDeuda"]').val();
    const idListado = $(this).data("id");
    const idUsuario = $("#IdConsumidor").val();
  

    if (!idListado) {
        showError("ID de deuda no válido.");
        return;
    }
    $.ajax({
        url: '/Deudor/Abonos',
        type: "POST",
        data: { idListado, abonoo, idUsuario  },
    }).done(function (data) {
        alert("Abono realizado")
    }).fail(function (xhr) {
        alert(`hubo un error en ${xhr.responseText}`);
    });
})

$("#DeudasTableBody").on("click", ".btn-detalles", function () {
    const idListado = $(this).data("id");

    if (!idListado) {
        showError("ID de deuda no válido.");
        return;
    }

    $.ajax({
        url: '/Deudor/ObtenerDetallesDeDeuda',
        type: "GET",
        data: { idListado },
    })
        .done(function (detalles) {
            const tbody = $("#DetallesTableBody");
            $('#TituloDetalles').text(`Detalles de la Deuda de ${electrcista} y ${consumidor}`)
            tbody.empty();
            console.log(detalles);
            if (!detalles || detalles.length === 0) {
                alert("No se encontraron detalles para esta deuda.");
                $("#DetallesContainer").hide();
                return;
            }
           
            detallesAbono = Array.isArray(detalles) ? detalles : [];

            detalles.forEach((d, index) => {
                tbody.append(`
                        <tr>
                            <td>${index + 1}</td>
                            <td>${d.producto}</td>
                            <td>${d.cantidad}</td>
                            <td>${d.precio.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                            <td>${d.total.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                            
                        </tr>
                    `);
            });
            actualizarTotalEnTabla();
            $("#DetallesContainer").show();
        })
        .fail(() => showError("Error al obtener los detalles de la deuda."));
});
$("#DeudasTableBody").on("click", ".btn-AbonoDetalles", function () {
    const idListado = $(this).data("id")
    if (!idListado) {
        showError("ID de deuda no válido.");
        console.log(`error en la id que es ${idListado}`)
        return;
    }

    $.ajax({
        url: '/Deudor/DetalleAbonos',
        type: "GET",
        data: { idListado }
    }).done(function (response) {
        const tbody = $("#AbonosTableBody");
        tbody.empty();

        const datos = response.datos;

        // 💾 Almacenar los datos en la variable global
        detallesAbono = Array.isArray(datos) ? datos : [];
        if (detallesAbono.length === 0) {
            showError(response.mensaje);
        } else {
            detallesAbono.forEach((d, index) => {
                tbody.append(`
                <tr>
                    <td>${index + 1}</td>
                    <td>${d.name}</td>
                    <td>${d.abonado}</td>
                    <td>${d.fechaAbono}</td>
                    <td>${d.total}</td>
                </tr>
            `);
            });
            actualizarTotalEnTabla();
            $("#AbonosContainer").show();

            // Mostrar en consola para verificar
        }
    });

})

$('#imprimir').on('click', function (e) {
    e.preventDefault();
    console.log(detallesAbono);
    const detallesAbonadosArray = Object.values(detallesAbono);
    console.log('detalles para impresion',detallesAbonadosArray)
    if (detallesAbonadosArray.length === 0) {
        alert('No hay detallesAbonados para generar un recibo.');
        return;
    }

    // Agrupar productos por idConsumidor
    const agrupadoPorConsumidor = {};

    detallesAbonadosArray.forEach(reg => {
        const id = reg.idConsumidor;
        if (!agrupadoPorConsumidor[id]) {
            agrupadoPorConsumidor[id] = {
                NombreCliente: $('#ConsumidorInput').val() || `Consumidor ${id}`,
                Fecha: new Date().toISOString(),
                Abonado: []
            };
        }

        agrupadoPorConsumidor[id].Abonado.push({
            Id: reg.id,
            FechaAbono: reg.fechaAbono,
            AbonoDeuda: reg.abonado,
        });
    });

    const consumidores = Object.values(agrupadoPorConsumidor);

    // Crear cuadro de diálogo de confirmación
    const dialogHtml = `
        <div id="confirmPrintDialog" class="modal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Confirmar Impresión</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <p><strong>Número de consumidores:</strong> ${consumidores.length}</p>
                        <p>¿Deseas proceder con la impresión de los recibos?</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <button type="button" id="confirmPrintButton" class="btn btn-primary">Imprimir</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Agregar el cuadro de diálogo al DOM
    $('body').append(dialogHtml);
    const modal = new bootstrap.Modal(document.getElementById('confirmPrintDialog'));
    modal.show();

    // Manejar la confirmación
    $('#confirmPrintButton').on('click', function () {
        modal.hide();
        $('#confirmPrintDialog').remove();

        consumidores.forEach((reciboData, index) => {
            $.ajax({
                url: '/Cotizacion/ImprimirAbonos',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(reciboData),
                xhrFields: { responseType: 'blob' },
                success: function (data) {
                    const blob = new Blob([data], { type: 'application/pdf' });
                    const blobUrl = window.URL.createObjectURL(blob);
                    const newTab = window.open(blobUrl, '_blank');
                    if (newTab) {
                        newTab.focus();
                    } else {
                        alert('Por favor, habilita los pop-ups para esta página.');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error al generar el recibo:', status, error);
                    alert('Error al generar uno de los recibos.');
                }
            });
        });
    });

    $('#confirmPrintDialog').on('hidden.bs.modal', function () {
        $(this).remove();
    });
});

function calcularTotalDeuda() {
    var total = 0;
    for (let clave in registros) {
        total += registros[clave].deuda;
    }
    return total;
}

function actualizarTotalEnTabla() {
    // Elimina fila de total anterior si ya existe
    $('#filaTotal').remove();

    var total = calcularTotalDeuda();
    if (total > 0) {
        $('#DetallesTableBody').append(`
<tr id="filaTotal">
    <td colspan="4" class="text-end"><strong>Total</strong></td>
    <td><strong>${total.toFixed(2)}</strong></td>
    <td></td>
</tr>`);
    }
}
