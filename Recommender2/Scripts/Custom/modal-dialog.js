$(document)
    .ready(function() {
        // Get the modal
        var modal = document.getElementById('myModal');

        // Get the button that opens the modal
        var btn = document.getElementById("modalOpen");

        // Get the <span> element that closes the modal
        var span = document.getElementsByClassName("close")[0];

        // When the user clicks on the button, open the modal 
        btn.onclick = function () {
            modal.style.display = "block";
        }

        // When the user clicks on <span> (x), close the modal
        span.onclick = function () {
            modal.style.display = "none";
        }

        // When the user clicks anywhere outside of the modal, close it
        window.onclick = function (event) {
            if (event.target == modal) {
                modal.style.display = "none";
            }
        }

        $('.checkAll').click(function () {
            var parentDiv = $(this).parent();
            var d = $(this).data(); // access the data object of the button
            parentDiv.find(':checkbox').prop('checked', !d.checked); // set all checkboxes 'checked' property using '.prop()'
            d.checked = !d.checked; // set the new 'checked' opposite value to the button's data object
        });

    });