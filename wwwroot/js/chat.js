
var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
    console.log(connection);
}).catch(function (err) {
    return console.error(err.toString());
});

let send_form;

connection.on("ReceiveMessage", function (data_msg) {
    if (data_msg.success) {
        // Append the new message to the message box
        var newMessage = `
                 <div class="message mb-3" id="msg-${data_msg.message.Id}">
                     <strong>${data_msg.message.userName}:</strong> ${data_msg.message.content}
                     <br />
                     <small class="text-muted">${data_msg.message.dateTime}</small>
                  </div>
                `;
        $("#msg-box-" + data_msg.message.channelId).prepend(newMessage);
        $("#msg-box-" + data_msg.message.channelId).scrollTop(0);
        send_form.find("input[name='message']").val(''); // Clear the input field
    } else {
        alert(data_msg.error_msg);
    }

});

$(document).ready(function () {
    $(".JoinForm").submit(function(e) {
        var form = $(this); 
        var groupId = form.attr("action").split("/")[3];
        //alert(groupId);
        connection.invoke("JoinGroup", groupId).catch(function (err) {
            return console.error(err.toString());
        });
    })

    $("form[id^='messageForm']").submit(function (e) {
        e.preventDefault(); // Prevent the default form submission

        var form = $(this); 
        send_form = form;
        var message = form.find("input[name='message']").val();
        var channelId = form.find("input[name='channelId']").val();
        //console.log('Message:', message);
        $.ajax({
            url: '/Channels/SendMessage',
            type: 'POST',
            data: {
                channelId: channelId,
                message: message
            },
            dataType: "json",
            success: function (data_msg) {
                console.log(data_msg);
                connection.invoke("SendMessageToGroup", data_msg.message.groupId.toString(), data_msg).catch(function (err) {
                    return console.error(err.toString());
                });
                /*
                if (data.success) {
                    var newMessage = `
                            <div class="message mb-3" id="msg-${data.message.Id}">
                                <strong>${data.message.userName}:</strong> ${data.message.content}
                                <br />
                                <small class="text-muted">${data.message.dateTime}</small>
                            </div>
                        `;
                    
                    $("#msg-box-" + channelId).prepend(newMessage);
                    $("#msg-box-" + channelId).scrollTop(0);
                    form.find("input[name='message']").val(''); 
                } else {
                    alert(data.error_msg);
                } */
            },
            error: function () {
                alert("An error occurred. Please try again.");
            }
        });
    });

    $(".message").on("contextmenu", function (e) {
        e.preventDefault(); // Prevent the default context menu
        // Get the message ID
        const messageId = e.target.id.split("-")[1];
        console.log(messageId);
        $("#menu-" + messageId).css({
            display: "block",
            top: "20px",
            left: e.pageX + "px"
            //transform: none
        });
        
        // Show the dropdown
        $('#btn-'+messageId).click();
    });

    // Hide context menu on clicking elsewhere
    $(document).on("click", function (e) {
        if (!$(e.target).closest('.dropdown').length) {
            $('.contextMenu').hide();
        }
    });

    // Edit message event
    $(document).on("click", ".editMessage", function (e) {
        const messageId = e.target.id;
        alert(`Editing message with ID: ${messageId}`);
        $('.contextMenu').hide();
    });

    // Delete message event
    $(document).on("click", ".deleteMessage", function (e) {
        const messageId = e.target.id;
        $('.contextMenu').hide();
    });
});
