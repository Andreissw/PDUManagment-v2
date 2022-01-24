/// <reference path="jquery-3.4.1.min.js" />




function AddList(object, name) {
    var list = $("#LP" + name);

    $("#LP" + name + " option").remove();

    for (const item of object.files) {
        list.append('<option>' + item.name + '</option>');
    }
   
}

function SetName(object) {

    $("#SpecBOM").val(object.files[0].name);
 
}

function getVisible(value) {
   
    $.ajax({
        url: "/CreateLOT/GetListModels",
        dataType: "json",
        data: { Name: value },
        success: function (data) {
            var m = $("#Model");
            m.empty();
            $.each(data, function () {
               m.append($("<option></option>").val(this['Value']).html(this['Text']))
            })
        },
    })

}





