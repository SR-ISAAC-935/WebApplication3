

$('form').on('submit', function (e) {
    e.preventDefault();
    var nitValue = $('input[name="Nit"]').val().trim(); // Asegúrate de que tu input Nit tenga el atributo name="Nit"
    console.log(nitValue.length);
    alert(nitValue);
    $.ajax({
        url: '/Facturas/ConsultaNit', // Cambia la URL a la acción correcta
        method: 'GET',
        contentType: 'application/json', // No es necesario para GET con datos en la URL
        data: { nit: nitValue }, // Envía el valor del NIT como un parámetro en la URL
        success: function (response) {
            // Maneja la respuesta exitosa de tu контроллер
            console.log("Respuesta del servidor:", response);
           // alert("Respuesta del servidor: " + response); // Muestra la respuesta (puedes actualizar tu UI aquí)
        },
        error: function (error) {
            // Maneja los errores de la petición AJAX
            console.error("Error en la petición:", error);
            alert("Error en la petición.");
        }
    });
});