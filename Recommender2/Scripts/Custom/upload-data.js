$(document).ready(function () {

    $(".role").click(function () {
        var number = $(this).attr("sequence");
        var selectedValue = $(this).val();
        var affectedElement = $(".parent[sequence=" + number + "]");
        if (selectedValue === "2") {
            affectedElement.prop("disabled", true);
        } else {
            affectedElement.prop("disabled", false);
        }
    });

    $("#CsvFile").change(function () {
        var value = document.getElementById("CsvFile").value;
        var parts = value.split(".");
        var extension = parts[parts.length - 1];
        var elementToStyle = $(".column-separator");
        if (extension === "csv")
            elementToStyle.css("display", "block");
        else if (extension === "ttl")
            elementToStyle.css("display", "none");
        else {
            alert("Please upload .csv or .ttl file.");
        }
    });

});