﻿$(document).ready(function () {

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

    $("#prefill").click(function () {
        $(".type[sequence=0]").val(3);
        $(".type[sequence=1]").val(2);
        $(".type[sequence=2]").val(2);
        $(".type[sequence=3]").val(2);
        $(".type[sequence=4]").val(2);
        $(".type[sequence=5]").val(2);
        $(".type[sequence=6]").val(2);
        $(".type[sequence=7]").val(0);
        $(".type[sequence=8]").val(1);

        $(".role[sequence=0]").val(1);
        $(".role[sequence=1]").val(1);
        $(".role[sequence=2]").val(1);
        $(".role[sequence=3]").val(1);
        $(".role[sequence=4]").val(1);
        $(".role[sequence=5]").val(1);
        $(".role[sequence=6]").val(1);
        $(".role[sequence=7]").val(2);
        $(".role[sequence=8]").val(2);

        $(".parent[sequence=0]").val("Root treeDimension");
        $(".parent[sequence=1]").val("Date");
        $(".parent[sequence=2]").val("Date");
        $(".parent[sequence=3]").val("Root treeDimension");
        $(".parent[sequence=4]").val("Product");
        $(".parent[sequence=5]").val("Root treeDimension");
        $(".parent[sequence=6]").val("Country");
    });

});