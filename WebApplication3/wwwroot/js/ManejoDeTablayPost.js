// Variables globales
var registros = {};
var tempIdProducto = null;
var tempIdConsumidor = null;

// Función para agregar un nuevo registro
$('#agregarRegistro').on('click', function () {
    try {
        // Obtener valores de los campos
        var consumidor = $('#ConsumidorInput').val().trim();
        var idConsumidor = tempIdConsumidor;
        var producto = $('#ProductoInput').val().trim();
        var idProducto = tempIdProducto;
        var cantidad = parseFloat($('#Cantidad').val()) || 0;
        var precio = parseFloat($('#Precio').val()) || 0;
        var deuda = cantidad * precio;

        // Validaciones
        if (!idConsumidor || !idProducto || cantidad <= 0 || precio <= 0 || isNaN(cantidad) || isNaN(precio)) {
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
            registros[clave] = { id: idUnico, idConsumidor, idProducto, producto, cantidad, precio, deuda };

            // Añadir una nueva fila a la tabla
            $('#tablaRegistros').append(`
<tr data-id="${idUnico}" data-idconsumidor="${idConsumidor}" data-idproducto="${idProducto}">
    <td>${consumidor}</td>
    <td>${producto}</td>
    <td>
        ${cantidad}
        <button type="button" class="btn btn-warning btn-sm restarCantidad" style="margin-left: 5px;">-</button>
    </td>
    <td>${precio.toFixed(2)}</td>
    <td>${deuda.toFixed(2)}</td>
    <td><button type="button" class="btn btn-danger btn-sm eliminarRegistro">Eliminar</button></td>
</tr>
`);


        }
        // Limpiar los campos de entrada
        limpiarCampos();
    } catch (error) {
        console.error('Error:', error);
        alert('Ocurrió un error al agregar el registro.');
    }
});

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

// Evento para restar cantidad
$('#tablaRegistros').on('click', '.restarCantidad', function () {
    try {
        var fila = $(this).closest('tr');
        var idUnico = fila.data('id');

        // Buscar el registro correspondiente
        var registro = Object.values(registros).find(reg => reg.id === idUnico);

        if (registro) {
            if (registro.cantidad > 1) {
                registro.cantidad -= 1;
                registro.deuda = registro.cantidad * registro.precio;
                actualizarFila(fila, registro);
            } else {
                alert('La cantidad no puede ser menor que 1. Si desea eliminar el registro, use el botón de eliminar.');
            }
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Ocurrió un error al restar la cantidad.');
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