/// <reference path="jquery-3.4.1.min.js" />

/// <reference path="ajax.googleapis.comajaxlibsjqueryui1.12.1jquery-ui.min.js" />
/// <reference path="ajaxlibsjquery3.2.1jquery.js" />



function EditName(line) {
    alert(line);
}

function RemoveTR(line,id) {
    if (!confirm("Уверены, что хотите удалить документ?")) return;

    $.ajax({

        url: '@Url.Action("RemoveDocument", "Edit")',
        dataType: "json",

        success: function (data) {
            if (data == true) {
                RemoveRow(line);
            }
            else {
                alert('Ошибка');
            }
        },
        error: function (xhr, status, error) {
            alert(xhr + status + error);
        }
    });


   
}

function RemoveRow(line) {
    var tbl = line.parentNode.parentNode.parentNode;
    var row = line.parentNode.parentNode.rowIndex;

    tbl.deleteRow(row);
}
