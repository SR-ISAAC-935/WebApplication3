
// Función para buscar items (productos o consumidores)
function buscarItems(inputId, listId, url, idKey, nameKey, extraDataKey = null) {
    $(inputId).on('input', function () {
        var query = $(this).val().trim();
        if (query.length >= 2) {
            $.ajax({
                url: url + '?term=' + query,
                success: function (data) {
                    if (data.length > 0) {
                        var listItems = data.map(item => `
                            <li class="list-group-item" data-id="${item[idKey]}" data-extra="${extraDataKey ? item[extraDataKey] : ''}">
                                ${item[nameKey]}
                            </li>
                        `).join('');
                        $(listId).html(listItems).show();
                    } else {
                        $(listId).html('<li class="list-group-item">No se encontraron resultados</li>').show();
                    }
                },
                error: function () {
                    alert('Error al buscar datos. Inténtalo nuevamente.');
                }
            });
        } else {
            $(listId).hide();
        }
    });

    $(listId).on('click', 'li', function () {
        var selectedText = $(this).text().trim();
        var selectedId = $(this).data('id');
        var selectedExtra = $(this).data('extra');

        $(inputId).val(selectedText);
        $(inputId + 'Id').val(selectedId);

        if (inputId === '#ProductoInput') {
            tempIdProducto = selectedId;
            if (selectedExtra) $('#Precio').val(selectedExtra);
        } else if (inputId === '#ConsumidorInput') {
            tempIdConsumidor = selectedId;
        }

        $(listId).hide();
    });
}

// Inicializar las búsquedas
buscarItems('#ProductoInput', '#ProductoList', '/Deudor/BuscarProductos', 'idProducto', 'nombreProducto', 'precioProducto');
buscarItems('#ConsumidorInput', '#ConsumidorList', '/Deudor/BuscarConsumidor', 'idConsumidor', 'nombreConsumidor');
