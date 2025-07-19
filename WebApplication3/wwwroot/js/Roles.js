function carajito() {
    $.ajax({
        url: '/Consumidor/Roles',
        type: 'GET'
    }).done(function (data) {
        const rolSelect = $('#Rol');
        rolSelect.empty();
        rolSelect.append('<option value="">Seleccione un rol</option>');
        data.forEach(function (rol) {
            rolSelect.append(`<option value="${rol.id}">${rol.descripcion}</option>`);
        });
    });
}
$('#NewConsumer').on('click',function() {

    const button = document.getElementById('NewConsumer');
    const consumer = document.getElementById('Consumers');
    (consumer.style.display == 'none') ? (consumer.style.setProperty('display', 'block'), (button.style.setProperty('display', 'none'))) : ("", "");
})

$('#hider').on('click' ,function () {    
    const button = document.getElementById('NewConsumer');
    const consumer = document.getElementById('Consumers');
    (consumer.style.display == 'block') ? (consumer.style.setProperty('display', 'none'), (button.style.setProperty('display', 'block'))) : (button.style.setProperty('display', "none")), (button.style.setProperty('display', 'block'))

})
carajito();
