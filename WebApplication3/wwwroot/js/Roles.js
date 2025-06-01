function carajito() {
    $.ajax({
        url: '/Consumidor/Roles',
        type: 'GET'
    }).done(function (data) {
        console.log(data); // Asegúrate de que la respuesta tenga el formato correcto
        const rolSelect = $('#Rol');
        rolSelect.empty();
        rolSelect.append('<option value="">Seleccione un rol</option>');
        data.forEach(function (rol) {
            rolSelect.append(`<option value="${rol.id}">${rol.descripcion}</option>`);
        });
    });
}

carajito();
