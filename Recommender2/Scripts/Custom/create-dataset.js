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

    $(".uploadLink").click(function () {
        var elementToClose = $("#manualDefinition");
        var elementToOpen = $("#dsdUpload");
        elementToClose.css("display", "none");
        elementToOpen.css("display", "block");
    });

    $("#prefill").click(function () {
        $(".type[sequence=0]").val("DateTime");
        $(".type[sequence=1]").val("String");
        $(".type[sequence=2]").val("String");
        $(".type[sequence=3]").val("String");
        $(".type[sequence=4]").val("String");
        $(".type[sequence=5]").val("String");
        $(".type[sequence=6]").val("String");
        $(".type[sequence=7]").val("Int32");
        $(".type[sequence=8]").val("Double");

        $(".role[sequence=0]").val(1);
        $(".role[sequence=1]").val(1);
        $(".role[sequence=2]").val(1);
        $(".role[sequence=3]").val(1);
        $(".role[sequence=4]").val(1);
        $(".role[sequence=5]").val(1);
        $(".role[sequence=6]").val(1);
        $(".role[sequence=7]").val(2);
        $(".role[sequence=8]").val(2);

        $(".parent[sequence=0]").val("Root dimension");
        $(".parent[sequence=1]").val("Date");
        $(".parent[sequence=2]").val("Date");
        $(".parent[sequence=3]").val("Root dimension");
        $(".parent[sequence=4]").val("Product");
        $(".parent[sequence=5]").val("Root dimension");
        $(".parent[sequence=6]").val("Country");
    });

});