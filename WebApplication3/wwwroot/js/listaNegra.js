﻿var registros = {};
var tempIdProducto = null;
var tempIdConsumidor = null;
var tempidelectricista=null
$("#ListaNegra").on("click", function (e) {
    e.preventDefault();

    if (typeof registros === 'undefined' || Object.keys(registros).length === 0) {
        console.log("registros no está definido o está vacío");
        return;
    }

    // Convertir registros a arreglo
    const registrosArray = Object.values(registros);
    console.log(registrosArray)
    // Agrupar por idConsumidor
    const agrupadoPorConsumidor = {};

    registrosArray.forEach(reg => {
        const idConsumidor = reg.idConsumidor;
        const idElectricista = reg.idElectricista;
        
        if (!agrupadoPorConsumidor[idConsumidor]) {
            agrupadoPorConsumidor[idConsumidor] = {
                idElectricista: idElectricista,
                idConsumidor: idConsumidor,
                productos: [],
                deudaTotal: 0
            };
        }

        agrupadoPorConsumidor[idConsumidor].productos.push({
            idElectricista: reg.idElectricista,
            idProducto: reg.idProducto,
            cantidad: reg.cantidad,
            precio: reg.precio,
            deuda: reg.deuda
        });

        agrupadoPorConsumidor[idConsumidor].deudaTotal += reg.deuda;
    });

    const consumidores = Object.values(agrupadoPorConsumidor);
    console.log("JSON agrupado para lista negra:", JSON.stringify(consumidores));
    const requestData = {
        ConsumidoresJson: JSON.stringify(consumidores), // Convertir a string JSON
        DeudaTotal: consumidores.reduce((acc, consumidor) => acc + consumidor.deudaTotal, 0) // Calcular deuda total
    };
    console.log("Datos a enviar al backend:", requestData);
    // Enviar al backend
  $.ajax({
        url: '/Deudor/Crear',
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
  }).done(function (data) {
      console.log("Respuesta del servidor:", data);
       // alert(data.mensaje);
    }).fail(function (xhr) {
        alert(`Hubo un error: ${xhr.responseText}`);
        console.log(`Hubo un error: ${xhr.responseText}`)
    });
});
$("#Ventas").on("click", function (e) {
    e.preventDefault();
    if (typeof registros === 'undefined' || Object.keys(registros).length === 0) {
        console.log("registros no está definido o está vacío");
        return;
    }

    const registrosArray = Object.values(registros).filter(r => r.cantidad >= 1);
    if (registrosArray.length === 0) {
        alert("Todos los productos tienen cantidad menor a 1. Verifica los datos.");
        return;
    }

    const fechaVenta = new Date().toISOString(); // puedes adaptar formato si es necesario
    const ventas = registrosArray.map(reg => ({
        idElectricista: reg.idElectricista,
        IdConsumidor: reg.idConsumidor,
        Deuda: reg.deuda,
        FechaVenta: fechaVenta,
        IdProducto: reg.idProducto,
        Cantidad: reg.cantidad,
        Precio: reg.precio
    }));

    console.log("Ventas a enviar:", ventas);

    const request = {
        ConsumidoresJson: JSON.stringify(ventas),
        DeudaTotal: ventas.reduce((acc, v) => acc + v.Deuda, 0)
    };

    $.ajax({
        url: '/Ventas/RealizarVenta',
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(request),
        success: function (data) {
          //  alert(data.mensaje);
        },
        error: function (xhr) {
            alert(`Hubo un error: ${xhr.responseText}`);
            console.log(`Hubo un error: ${xhr.responseText}`)
        }
    });

});
