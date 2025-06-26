// Variables globales
var registros = [];
var tempIdProducto = null;
var tempIdConsumidor = null;

// Evento para guardar el recibo
$('#GuardarRecibo').on('click', function (e) {
    e.preventDefault();

    const registrosArray = Object.values(registros);

    if (registrosArray.length === 0) {
        alert('No hay registros para generar un recibo.');
        return;
    }

    // Agrupar productos por consumidor
    const agrupadoPorConsumidor = {};

    registrosArray.forEach(reg => {
        const id = reg.idConsumidor;
        if (!agrupadoPorConsumidor[id]) {
            agrupadoPorConsumidor[id] = {
                nombreCliente: $('#ConsumidorInput').val(), // Esto deberías adaptar si manejas múltiples nombres
                fecha: new Date().toISOString(),
                productos: []
            };
        }

        agrupadoPorConsumidor[id].productos.push({
            Codigo: reg.idProducto,
            Descripcion: reg.producto,
            Cantidad: reg.cantidad,
            PrecioUnitario: reg.precio,
        });
    });

    const consumidores = Object.values(agrupadoPorConsumidor);

    // Crear cuadro de diálogo para nombre de archivo
    const dialogHtml = `
        <div id="saveFileDialog" class="modal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Guardar Recibo</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <label for="fileNameInput" class="form-label">Nombre base del archivo:</label>
                        <input type="text" id="fileNameInput" class="form-control" placeholder="Ejemplo: Recibo_Cliente" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <button type="button" id="saveFileButton" class="btn btn-primary">Guardar</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('body').append(dialogHtml);
    const modal = new bootstrap.Modal(document.getElementById('saveFileDialog'));
    modal.show();

    $('#saveFileButton').on('click', function () {
        const fileNameBase = $('#fileNameInput').val().trim();

        if (!fileNameBase) {
            alert('Por favor, ingresa un nombre base para el archivo.');
            return;
        }

        modal.hide();
        $('#saveFileDialog').remove();

        // Enviar y generar un recibo por consumidor
        consumidores.forEach((reciboData, index) => {
            $.ajax({
                url: '/Cotizacion/ImprimirRecibos',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(reciboData),
                xhrFields: { responseType: 'blob' },
                success: function (data) {
                    const blob = new Blob([data], { type: 'application/pdf' });
                    const link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.click();
                },
                error: function (xhr, status, error) {
                    console.error('Error al generar el recibo:', status, error);
                    alert('Error al generar el recibo.');
                }
            });
        });
    });

    $('#saveFileDialog').on('hidden.bs.modal', function () {
        $(this).remove();
    });
});

$('#imprimir').on('click', function (e) {
    e.preventDefault();

    const registrosArray = Object.values(registros);

    if (registrosArray.length === 0) {
        alert('No hay registros para generar un recibo.');
        return;
    }

    // Agrupar productos por idConsumidor
    const agrupadoPorConsumidor = {};

    registrosArray.forEach(reg => {
        const id = reg.idConsumidor;
        if (!agrupadoPorConsumidor[id]) {
            agrupadoPorConsumidor[id] = {
                nombreCliente: $('#ConsumidorInput').val() || `Consumidor ${id}`, // Puedes adaptar esto
                fecha: new Date().toISOString(),
                productos: []
            };
        }

        agrupadoPorConsumidor[id].productos.push({
            Codigo: reg.idProducto,
            Descripcion: reg.producto,
            Cantidad: reg.cantidad,
            PrecioUnitario: reg.precio
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
                url: '/Cotizacion/ImprimirRecibos',
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
