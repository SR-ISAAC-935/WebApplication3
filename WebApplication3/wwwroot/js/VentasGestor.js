$("#btnObtenerFacturas").on("click", function () {
    const idConsumidor = $("#IdConsumidor").val();
    const validationMessage = $("#IdConsumidorValidation");
    alert(idConsumidor)
    if (!idConsumidor) {
        validationMessage.text("Por favor, seleccione un consumidor válido.");
        return;
    }
    validationMessage.text("");

    const button = $(this).prop("disabled", true).text("Cargando...");
    let tots = 0;
    $.ajax({
        url: '/ResumenVentas/VentasElect',
        type: "GET",
        data: { idConsumidor },
    })
        .done(function (data) {
            const tbody = $("#DetallesVentasTableBody");
            tbody.empty();

            if (!data || data.length === 0) {
                alert("No se encontraron deudas para este consumidor.");
                $("#VentasRealizadas").hide();
                return;
            }
            console.log(data)
            data.forEach(d => {
                tbody.append(`
                        <tr>
                        <td>${d.idSales}</td>
                            <td>${d.nombreConsumidor}</td>
                            <td>${d.deuda.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                             <td>${new Date(d.fechaVenta).toLocaleDateString()}</td>
                        </tr>
                    `);
                tots = d.total;
            });
            $("#DetallesVentasTableBody").append(`<tr><td> Total de compras realizadas ${tots}</td></tr>`);
           
            $("#DetallesVentas").show();        // <- Mostrar la sección correcta
            $("#VentasRealizadas").hide();     // <- Ocultar la anterior si ya no se usa

        })
        .fail(() => showError("Error al obtener las deudas."))
        .always(() => button.prop("disabled", false).text("Obtener Facturas"));
});