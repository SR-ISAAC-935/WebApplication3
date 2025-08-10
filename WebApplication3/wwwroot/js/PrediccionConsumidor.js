
// Función para buscar items (productos o consumidores)
var dataItems = [];
let selectedIndex = -1;
function buscarItems(inputId, listId, url, idKey, nameKey, extraDataKey = null, providerKey = null,buyedkey=null, roleKey =null,stock=null) {
    $(inputId).on('input', function () {
        var query = $(this).val().trim();
        if (query.length >= 2) {
            $.ajax({
                url: url + '?term=' + query,
                success: function (data) {
                    if (data.length > 0) {
                        console.log(data);
                        dataItems = data; // Guardar los datos para uso posterior
                       var  listItems = data.map(item => `
<li class="list-group-item"
    data-id="${item[idKey]}"
    data-extra="${extraDataKey ? item[extraDataKey] : ''}"
    data-provider="${providerKey ? item[providerKey] : ''}"
    data-role="${roleKey ? item[roleKey] : ''}"
   data-stock="${item['stock']}"
   data-buyed="${item[buyedkey]}"
    data-nombrevisible="${item[nameKey]}">
    ${item[nameKey]}<br>
    <small class="text-muted">
        ${providerKey ? item[providerKey] : ''}
        ${roleKey ? '<br><em>' + item[roleKey] + '</em>' : ''}
    </small>
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
    function updateSelection() {
        const items = $(listId).find('li');
        items.removeClass('active');
        if(selectedIndex >= 0 && selectedIndex < items.length) {
            $(items[selectedIndex]).addClass('active');
            items.eq(selectedIndex).addClass('active');
        }
    }
    updateSelection();
    document.addEventListener("keydown", (e) => {
        const items = $(listId).find('li');
        if (items.length === 0) return;
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            selectedIndex = (selectedIndex + 1) % items.length;
            updateSelection();
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            selectedIndex = (selectedIndex - 1 + items.length) % items.length;
            updateSelection();
        } else if (e.key === 'Enter' && selectedIndex >= 0) {
            e.preventDefault();
            items.eq(selectedIndex).click(); // Opcional: puedes simular clic o extraer datos
        }
    })
    $(listId).on('click', 'li', function () {
        var selectedText = $(this).data('nombrevisible');
        var selectedId = $(this).data('id');
        var selectedExtra = $(this).data('extra');
        var provider = $(this).data('provider');
        var buyed =$(this).data('buyed')
        var role = $(this).data('role');
        var stock = $(this).data('stock');
        //var stock = $(this).data('stock');
        $('#dispo').text(`U: ${stock}  /  Comprado: Q.${buyed}`).show();
        $('#Comprado').text(` /`).show();
        $('#stocking').text(stock).show();
        $(inputId).val(selectedText);
        $(inputId + 'Id').val(selectedId);
        if (inputId === '#ElectricistaInput') {
            tempIdElectricista = selectedId;
            $('#IdElectricista').val(selectedId);
        } else if (inputId === '#ConsumidorInput') {
            tempIdConsumidor = selectedId;
            $('#IdConsumidor').val(selectedId);
        } else if (inputId === '#ProductoInput') {
            tempIdProducto = selectedId;
            if (selectedExtra) {
                $('#Precio').val(selectedExtra);
            }
            $('#IdProducto').val(selectedId);
        }


        $(listId).hide();
    });
}


// Inicializar las búsquedas
buscarItems('#ProductoInput', '#ProductoList', '/Deudor/BuscarProductos', 'idProducto', 'nombreProducto', 'precioProducto', 'productProvider', 'productBuyed','stock');
buscarItems(
    '#ConsumidorInput',
    '#ConsumidorList',
    '/Deudor/BuscarConsumidor',
    'idConsumidor',
    'nombreConsumidor',
    null,          // extraDataKey
    null,          // providerKey
    'role'         // roleKey
);
buscarItems(
    '#ElectricistaInput',
    '#ElectricistaList',
    '/Deudor/BuscarConsumidor',
    'idConsumidor',
    'nombreConsumidor',
    null,       // extraDataKey si tienes
    null,       // providerKey si tienes
    'role',      // roleKey si tienes
   
);
