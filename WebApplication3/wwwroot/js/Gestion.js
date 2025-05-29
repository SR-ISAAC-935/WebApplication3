function showError(message) {
    alert(message);
    console.error(message);
}

$("#ConsumidorInput").on("input", function () {
    const term = $(this).val();
    if (term.length < 3) {
        $("#ConsumidorList").hide();
        return;
    }

    $.get(`/Deudor/BuscarConsumidor?term=${encodeURIComponent(term)}`)
        .done(function (data) {
            const list = $("#ConsumidorList");
            list.empty();
            if (data && data.length > 0) {
                data.forEach(c => {
                    list.append(`<li class="list-group-item" data-id="${c.idConsumidor}">${c.nombreConsumidor}</li>`);
                });
                list.show();
            } else {
                list.hide();
            }
        })
        .fail(() => showError("Error al buscar consumidores."));
});

$("#ConsumidorList").on("click", ".list-group-item", function () {
    const id = $(this).data("id");
    $("#ConsumidorInput").val($(this).text());
    $("#IdConsumidor").val(id);
    $("#ConsumidorList").hide();
});

$("#btnObtenerFacturas").on("click", function () {
    const idConsumidor = $("#IdConsumidor").val();
    const validationMessage = $("#IdConsumidorValidation");

    if (!idConsumidor) {
        validationMessage.text("Por favor, seleccione un consumidor válido.");
        return;
    }
    validationMessage.text("");

    const button = $(this).prop("disabled", true).text("Cargando...");

    $.ajax({
        url: '/Deudor/ObtenerDeudasPorConsumidor',
        type: "GET",
        data: { idConsumidor },
    })
        .done(function (data) {
            const tbody = $("#DeudasTableBody");
            tbody.empty();

            if (!data || data.length === 0) {
                alert("No se encontraron deudas para este consumidor.");
                $("#DeudasContainer").hide();
                return;
            }

            data.forEach(d => {
                tbody.append(`
                        <tr>
                            <td>${d.deuda.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                            <td>${new Date(d.fechaVenta).toLocaleDateString()}</td>
                            <td><button class="btn btn-primary btn-detalles" data-id="${d.idListado}">Ver detalles</button></td>
                             <td><input type="text" name="AbonoDeuda" class="AbonoDeuda" placeholder="Ingrese abono a deuda" /></td>
                     <td><button class="btn btn-danger btn-Cancelar" data-id="${d.idListado}">Abonar Deuda</button></td>
                      <td><button class="btn btn-primary btn-AbonoDetalles" data-id="${d.idListado}">Detalles Abonos</button></td>
                        </tr>
                    `);
            });
            $("#DeudasContainer").show();
        })
        .fail(() => showError("Error al obtener las deudas."))
        .always(() => button.prop("disabled", false).text("Obtener Facturas"));
});

$("#DeudasTableBody").on("click", ".btn-Cancelar", function () {
    const fila = $(this).closest("tr");
    const abono = fila.find('input[name="AbonoDeuda"]').val();
    const idListado = $(this).data("id");

    alert(`Se llamó la id ${idListado}, valor de abono es de ${abono}`);

    if (!idListado) {
        showError("ID de deuda no válido.");
        return;
    }
    $.ajax({
        url: '/Deudor/actualizarDeuda',
        type: "PUT",
        data: { idListado, abono },
    }).done(function (data) {
        console.log(data)
        alert(data.mensaje)
        location.reload();
    }).fail(function (xhr) {
        console.log(`error en ${xhr.responseText}`);
        alert(`hubo un error en ${xhr.responseText}`);
    });

});
$("#DeudasTableBody").on("click", ".btn-Cancelar", function () {
    const fila = $(this).closest("tr");
    const abonoo = fila.find('input[name="AbonoDeuda"]').val();
    const idListado = $(this).data("id");
    const idUsuario = $("#IdConsumidor").val();
    alert(`Se llamó la id ${idListado}, valor de abono es de ${abonoo}, y la id${idUsuario}`);

    if (!idListado) {
        showError("ID de deuda no válido.");
        return;
    }
    $.ajax({
        url: '/Deudor/Abonos',
        type: "POST",
        data: { idListado, abonoo, idUsuario  },
    }).done(function (data) {
        console.log(data)
        alert(data.mensaje)
       // location.reload();
    }).fail(function (xhr) {
        console.log(`error en ${xhr.responseText}`);
        alert(`hubo un error en ${xhr.responseText}`);
    });
})

$("#DeudasTableBody").on("click", ".btn-detalles", function () {
    const idListado = $(this).data("id");

    if (!idListado) {
        showError("ID de deuda no válido.");
        return;
    }

    $.ajax({
        url: '/Deudor/ObtenerDetallesDeDeuda',
        type: "GET",
        data: { idListado },
    })
        .done(function (data) {
            const tbody = $("#DetallesTableBody");
            tbody.empty();

            if (!data || data.length === 0) {
                alert("No se encontraron detalles para esta deuda.");
                $("#DetallesContainer").hide();
                return;
            }

            data.forEach(d => {
                tbody.append(`
                        <tr>
                            <td>${d.producto}</td>
                            <td>${d.cantidad}</td>
                            <td>${d.precio.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                            <td>${d.total.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                             <td><button class="btn btn-danger btn-AbonoDetalles"  data-id="${idListado}">Detalle Abonos</button></td>
                        </tr>
                    `);
            });
            $("#DetallesContainer").show();
        })
        .fail(() => showError("Error al obtener los detalles de la deuda."));
});
$("#DeudasTableBody").on("click", ".btn-AbonoDetalles", function () {
    const idListado = $(this).data("id")
    console.log(`listado interno es ${idListado}`)
    if (!idListado) {
        showError("ID de deuda no válido.");
        console.log(`error en la id que es ${idListado}`)
        return;
    }

    $.ajax({
        url: '/Deudor/DetalleAbonos',
        type: "GET",
        data: { idListado }
    }).done(function (response) {
        console.log("Respuesta completa:", response);
        const tbody = $("#AbonosTableBody");
        tbody.empty();

        const datos = response.datos; // <- Aquí se corrige el error
        if (Array.isArray(datos)) {
            console.log(`Cantidad de abonos: ${datos.length}`);

            if (datos.length === 0) {
                showError(response.mensaje);
            } else {
                datos.forEach((d, index) => {
                    tbody.append(`
                    <tr>
                        <td>${index + 1}</td>
                        <td>${d.name}</td>
                        <td>${d.abonado}</td>
                        <td>${d.fechaAbono}</td>
                        <td>${d.total}</td>
                    </tr>
                `);
                });

                $("#AbonosContainer").show();
                datos.forEach((abono, index) => {
                    console.log(`Abono #${index + 1}:`, abono);
                });
            }
        } else {
            showError("Respuesta inesperada del servidor.");
        }
    });



})