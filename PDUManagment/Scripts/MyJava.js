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
    var obj = document.getElementById('OrderDiv');
    if (value == "Контрактное") {
        obj.style.visibility = "visible";
    }
    else {
        obj.style.visibility = "hidden";
        obj.value = '';
    }

}


var valueResult = document.getElementById('Client').value;

if (valueResult == "Контрактное") {
    document.getElementById('OrderDiv').style.visibility = "visible";
}
else {
    document.getElementById('OrderDiv').style.visibility = "hidden";
}



