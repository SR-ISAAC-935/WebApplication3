// Variables globales
var registros = {};
var tempIdProducto = null;
var tempIdConsumidor = null;
var tempIdElectricista = null;
// Función para agregar un nuevo registro
$('#agregarRegistro').on('click', function (e) {
    e.preventDefault();
    try {
        // Obtener valores de los campos
        var consumidor = $('#ConsumidorInput').val().trim();
        var electricista = $('#ElectricistaInput').val().trim();
        var idElectricista = tempIdElectricista
        var idConsumidor = tempIdConsumidor;
        var producto = $('#ProductoInput').val().trim();
        var idProducto = tempIdProducto;
        var cantidad = parseFloat($('#Cantidad').val()) || 0;
        var precio = parseFloat($('#Precio').val()) || 0;
        var deuda = cantidad * precio;
        console.log($`consu${consumidor}, elec${idElectricista}`);
        // Validaciones
        if (cantidad <= 0 || precio <= 0 || isNaN(cantidad) || isNaN(precio)) {
            alert('Por favor, completa los campos correctamente.');
            return;
        }

        // Clave única para el registro
        var clave = `${idConsumidor}-${idProducto}`;

        // Verificar si ya existe un registro
        if (registros[clave]) {
            // Actualizar el registro existente
            registros[clave].cantidad += cantidad;
            registros[clave].deuda = registros[clave].cantidad * registros[clave].precio;

            // Actualizar la fila correspondiente en la tabla
            var filaExistente = $(`#tablaRegistros tr[data-id="${registros[clave].id}"]`);
            actualizarFila(filaExistente, registros[clave]);
        } else {
            // Crear un nuevo registro
            var idUnico = Date.now();
            registros[clave] = { id: idUnico, idConsumidor, idElectricista, idProducto, producto, cantidad, precio, deuda };

            // Añadir una nueva fila a la tabla
            $('#tablaRegistros').append(`
<tr data-id="${idUnico}" data-idconsumidor="${idConsumidor}" data-idproducto="${idProducto}">
    <td>${consumidor}</td>
    <td>${producto}</td>
    <td>
     <button type="button" class="btn btn-warning btn-sm sumarCantidad" style="margin-left: 5px;">+</button>
       <span class="cantidad"> ${cantidad}</span>
        <button type="button" class="btn btn-warning btn-sm restarCantidad" style="margin-left: 5px;">-</button>
    </td>
    <td>${precio.toFixed(2)}</td>
    <td>${deuda.toFixed(2)}</td>
    <td><button type="button" class="btn btn-danger btn-sm eliminarRegistro">Eliminar</button></td>
</tr>
`);
            actualizarTotalEnTabla();

        }
        // Limpiar los campos de entrada
        limpiarCampos();
    } catch (error) {
        console.error('Error:', error);
        alert('Ocurrió un error al agregar el registro.');
    }
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
        $('#tablaRegistros').append(`
<tr id="filaTotal">
    <td colspan="4" class="text-end"><strong>Total</strong></td>
    <td><strong>${total.toFixed(2)}</strong></td>
    <td></td>
</tr>`);
    }
}

// Función para actualizar una fila en la tabla
function actualizarFila(fila, registro) {
    fila.find('td:eq(2)').text(registro.cantidad); // Actualizar cantidad
    fila.find('td:eq(4)').text(registro.deuda.toFixed(2)); // Actualizar deuda
}

// Función para limpiar los campos de entrada
function limpiarCampos() {
    $('#ProductoInput').val('');
    $('#Cantidad').val('');
    $('#Precio').val('');
}

//// Evento para restar cantidad
$(document).on('click', '.restarCantidad', function () {
    try {
        var fila = $(this).closest('tr');
        var idUnico = fila.data('id');

        // Buscar el registro correspondiente
        var registro = Object.values(registros).find(reg => reg.id === idUnico);

        if (registro) {
            registro.cantidad -= 1;
            registro.deuda = registro.cantidad * registro.precio;

            // Actualizar cantidad y deuda en la fila
            fila.find('.cantidad').text(registro.cantidad);
            fila.find('td').eq(4).text(registro.deuda.toFixed(2)); // deuda está en la columna 4
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Ocurrió un error al sumar la cantidad.');
    }
});

$(document).on('click', '.sumarCantidad', function () {
    try {
        var fila = $(this).closest('tr');
        var idUnico = fila.data('id');

        // Buscar el registro correspondiente
        var registro = Object.values(registros).find(reg => reg.id === idUnico);

        if (registro) {
            registro.cantidad += 1;
            registro.deuda = registro.cantidad * registro.precio;

            // Actualizar cantidad y deuda en la fila
            fila.find('.cantidad').text(registro.cantidad);
            fila.find('td').eq(4).text(registro.deuda.toFixed(2)); // deuda está en la columna 4
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Ocurrió un error al sumar la cantidad.');
    }
});

// Evento para eliminar registro
$('#tablaRegistros').on('click', '.eliminarRegistro', function () {
    try {
        var fila = $(this).closest('tr');
        var idUnico = fila.data('id');

        // Eliminar el registro del objeto
        var clave = Object.keys(registros).find(key => registros[key].id === idUnico);
        if (clave) {
            delete registros[clave];
        }

        // Eliminar la fila de la tabla
        fila.remove();
    } catch (error) {
        console.error('Error:', error);
        alert('Ocurrió un error al eliminar el registro.');
    }
});
