
$(document).ready(function () {
    $("form[id^='messageForm']").submit(function (e) {
        e.preventDefault(); // Prevent the default form submission

        var form = $(this); // Get the current form
        var message = form.find("input[name='message']").val();
        var channelId = form.find("input[name='channelId']").val();
        console.log('Message:', message);
        $.ajax({
            url: '/Channels/SendMessage',
            type: 'POST',
            data: {
                channelId: channelId,
                message: message
            },
            dataType: "json",
            success: function (data) {
                if (data.success) {
                    console.log(data);
                    // Append the new message to the message box
                    var newMessage = `
                            <div class="message mb-3" id="message-${data.message.id}">
                                <strong>${data.message.userName}:</strong> ${data.message.content}
                                <br />
                                <small class="text-muted">${data.message.dateTime}</small>
                            </div>
                        `;
                    
                    $("#msg-box-" + channelId).prepend(newMessage);
                    $("#msg-box-" + channelId).scrollTop(0);
                    form.find("input[name='message']").val(''); // Clear the input field
                } else {
                    alert("Failed to send message. Please try again.");
                }
            },
            error: function () {
                alert("An error occurred. Please try again.");
            }
        });
    });

    $(".message").on("contextmenu", function (e) {
        e.preventDefault(); // Prevent the default context menu
        //alert(e.pageY);
        // Get the message ID
        const messageId = $(this).data("message-id");
        
        // Position the dropdown (context menu) based on mouse click
        $("#contextMenu").css({
            display: "block",
            top: e.pageY + "px",
            left: e.pageX + "px"
            //width: 50 + "px"
        });

        // Store the message ID in the context menu
        $("#contextMenu").data("message-id", messageId);

        // Show the dropdown
        $('#contextMenuButton').click();
    });

    // Hide context menu on clicking elsewhere
    $(document).on("click", function (e) {
        if (!$(e.target).closest('.dropdown').length) {
            $('#contextMenu').hide();
        }
    });

    // Edit message event
    $(document).on("click", "#editMessage", function () {
        const messageId = $("#contextMenu").data("message-id");
        console.log("Edit Message:", messageId);
        // Trigger your edit functionality here
        alert(`Editing message with ID: ${messageId}`);
        $('#contextMenu').hide();
    });

    // Delete message event
    $(document).on("click", "#deleteMessage", function () {
        const messageId = $("#contextMenu").data("message-id");
        console.log("Delete Message:", messageId);
        // Trigger your delete functionality here
        alert(`Deleting message with ID: ${messageId}`);
        $('#contextMenu').hide();
    });
});
