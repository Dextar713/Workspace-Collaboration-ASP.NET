
//import BootstrapMenu from 'bootstrap-menu';

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
                 <div class="message mb-3" id="msg-${data_msg.message.id}">
                     <strong>${data_msg.message.userName}:</strong> ${data_msg.message.content}
                     <br />
                     <small class="text-muted">${data_msg.message.dateTime}</small>
                  </div>
                `;
        $("#msg-box-" + data_msg.message.channelId).prepend(newMessage);
        $("#msg-box-" + data_msg.message.channelId).scrollTop(0);
        send_form.find("input[name='message']").val(''); // Clear the input field
        setTimeout(() => {
            attachDropdown("#msg-" + data_msg.message.id);
        }, 100);
        //attachDropdown("#msg-"+data_msg.message.Id);
    } else {
        alert(data_msg.error_msg);
    }

});

connection.on("DeleteMessage", function (msgId) {
    var messageElement = $("#msg-" + msgId);
    messageElement.remove();
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
    attachDropdown(".message");

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
            },
            error: function () {
                alert("An error occurred. Please try again.");
            }
        });
    });
    
});

function attachDropdown(elem) {
    document.querySelectorAll(elem).forEach((element) => {
        element.addEventListener("contextmenu", (event) => {
            event.preventDefault(); // Prevent default right-click behavior

            document.querySelectorAll(".dropdown-menu").forEach((menu) => menu.remove());

            // Create dropdown dynamically
            const dropdownMenu = document.createElement("div");
            dropdownMenu.className = "dropdown-menu show";
            dropdownMenu.style.position = "absolute";
            dropdownMenu.style.left = `${event.clientX + window.scrollX}px`;
            dropdownMenu.style.top = `${event.clientY + window.scrollY}px`;
            dropdownMenu.style.zIndex = 1050;

            // Add menu items
            dropdownMenu.innerHTML = `
                <button class="dropdown-item editMessage">Edit</button>
                <button class="dropdown-item deleteMessage">Delete</button>
            `;

            // Append to the body
            document.body.appendChild(dropdownMenu);

            // Add Bootstrap's Dropdown functionality
            const bootstrapDropdown = new bootstrap.Dropdown(dropdownMenu);

            // Attach event listeners for menu actions
            dropdownMenu.querySelector(".editMessage").addEventListener("click", () => {
                alert("Edited message with id: "+ element.id);
                dropdownMenu.remove();
            });

            dropdownMenu.querySelector(".deleteMessage").addEventListener("click", () => {
                //alert("Delete clicked for:", element.id);
                //console.log(element);
                var msgId = element.id.replace("msg-", "");
                //alert(msgId);
                
                $.ajax({
                    url: '/Messages/Delete',
                    type: 'POST',
                    data: {
                        id: msgId
                    },
                    dataType: "json",
                    success: function (res) {
                        if (res.success) {
                            console.log(res);
                            connection.invoke("DeleteMessage", msgId).catch(function (err) {
                                return console.error(err.toString());
                            });
                            //alert("Message deleted!");
                        } else {
                            alert(res.error_msg);
                        }

                    },
                    error: function () {
                        alert("An error occurred. Please try again.");
                    }
                });
                
                dropdownMenu.remove();
            });

            // Remove dropdown on outside click
            document.addEventListener("click", () => {
                dropdownMenu.remove();
            }, { once: true });
        });
    });

}
