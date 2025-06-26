$('#vefel').on('click', function () {
    $.ajax({
        url: '/Xml/DescargarRecibo',
        method: 'GET',
        xhr: function () {
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'blob';
            return xhr;
        },
        success: function (data) {
            const blob = new Blob([data], { type: 'application/pdf' });
            const blobUrl = window.URL.createObjectURL(blob);

            const newTab = window.open(blobUrl, '_blank');
            if (newTab) {
                newTab.focus();
            } else {
                alert('Por favor, habilita los pop-ups para esta página.');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error al generar el recibo:', status, error);
            alert('Error al generar uno de los recibos.');
        }
    });
});
