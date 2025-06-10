$(document).ready(function () {
    var consumidores = []; // Array para almacenar los consumidores agregados

    // Función para agregar un consumidor a la lista
    $('#agregarConsumidor').click(function () {
        var nombreConsumidor = $('#NombreConsumidor').val().trim();
        var rolId = $('#Rol').val();
        var rolNombre = $('#Rol option:selected').text();

        if (!nombreConsumidor || !rolId) {
            alert("Debe ingresar un nombre y seleccionar un rol.");
            return;
        }

        consumidores.push({
            NombreConsumidor: nombreConsumidor,
            RolNombre: rolNombre,
            IdRole: rolId
        });

        $('#consumidoresList').append(`
            <li>
                ${nombreConsumidor} - ${rolNombre}
                <button type="button" class="btn btn-danger btn-sm eliminarConsumidor">Eliminar</button>
            </li>
        `);

        $('#NombreConsumidor').val('');
        $('#Rol').val('');
    });

    // Enviar el formulario
    $('#formRegistrarConsumidores').submit(function (event) {
        event.preventDefault();

        if (consumidores.length > 0) {
            var consumidoresJson = JSON.stringify(consumidores);
            $.ajax({
                url: '/Consumidor/CrearConsumidores', // o usar @Url.Action(...) si estás en Razor
                type: 'POST',
                contentType: 'application/json',
                data: consumidoresJson,
                success: function (response) {
                    
                    $('#consumidoresList').empty();
                    consumidores = [];
                },
                error: function (xhr) {
                    let mensaje = "Ocurrió un error al guardar los consumidores.";
                    if (xhr.responseJSON && xhr.responseJSON.Mensaje) {
                        mensaje = xhr.responseJSON.Mensaje;
                    }
                    alert(mensaje);
                }
            });
        } else {
            alert("Debe agregar al menos un consumidor.");
        }
    });
});
