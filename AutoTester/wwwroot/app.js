const myUserId = messenger.getAttribute('data-mid') || '1';
const otherUserId = messenger.getAttribute('data-oid') || '2';

(async function() {
    const response = await fetch('/messages-between?a=' + myUserId + '&b=' + otherUserId, {
        method: 'get',
        headers: {
            'content-type': 'application/json; charset=UTF-8'
        }
    });

    try {
        const existingMessagesBetweenUs = await response.json();

        let existingMessagesHtml = ''

        for (const message of existingMessagesBetweenUs) {
            message.mine = message.fromUserId === myUserId

            existingMessagesHtml += generateMessageView(message);
        }

        messengerMessagingView.innerHTML = existingMessagesHtml;

        // scroll to the bottom
        messengerMessagingView.scrollTop = messengerMessagingView.scrollHeight;
    } catch (e) {
        console.error(e);
    }
})()

function generateMessageView(msg) {
    let sender = 'Me';

    if (!msg.mine) {
        sender = msg.fromUserId;
    }

    return `
            <div class="message-view ${msg.mine ? 'my-message' : ''}">
                <div class="message-header-view">
                    <div class="message-sender-view">${sender}</div>
                    <div class="message-created-date-view">${tiffDifference(msg.created)}</div>
                </div>
                <div class="message-content-view">${msg.content}</div>
            </div>
        `
}

function tiffDifference(previous) {
    const current = new Date().getTime();

    const msPerMinute = 60 * 1000;
    const msPerHour = msPerMinute * 60;
    const msPerDay = msPerHour * 24;
    const msPerMonth = msPerDay * 30;
    const msPerYear = msPerDay * 365;

    const elapsed = current - previous;

    if (elapsed < msPerMinute) {
        return Math.round(elapsed/1000) + ' seconds ago';
    } else if (elapsed < msPerHour) {
        return Math.round(elapsed/msPerMinute) + ' minutes ago';
    } else if (elapsed < msPerDay) {
        return Math.round(elapsed/msPerHour) + ' hours ago';
    } else if (elapsed < msPerMonth) {
        return Math.round(elapsed/msPerDay) + ' days ago';
    } else if (elapsed < msPerYear) {
        return Math.round(elapsed/msPerMonth) + ' months ago';
    }

    return Math.round(elapsed/msPerYear) + ' years ago';
}

sendBtn.addEventListener('click', async function() {
    const messageContent = sendInput.value;

    if (messageContent) {
        sendBtnIcon.classList.remove('fa-paper-plane')
        sendBtnIcon.classList.add('fa-circle-notch', 'fa-spin')

        const response = await fetch("/send-message", {
            method: 'post',
            headers: {
                'content-type': 'application/json; charset=UTF-8'
            },
            body: JSON.stringify({
                fromUserId: myUserId,
                toUserId: otherUserId,
                content: messageContent
            })
        });

        try {
            const persistedMessage = await response.json();
            persistedMessage.mine = true;
            messengerMessagingView.innerHTML += generateMessageView(persistedMessage);

            // scroll to the bottom
            messengerMessagingView.scrollTop = messengerMessagingView.scrollHeight;
        } catch (e) {
            console.log(e);
        }

        sendBtnIcon.classList.add('fa-paper-plane')
        sendBtnIcon.classList.remove('fa-circle-notch', 'fa-spin')
    }
})

const socket = new WebSocket("ws://localhost:5267/ws");

socket.onopen = function(e) {
    console.log("[open] Connection established");
    console.log("Sending to server");
    socket.send("My name is John");
};

socket.onmessage = function(e) {
    const inboundMessage = JSON.parse(e.data);
    messengerMessagingView.innerHTML += generateMessageView(inboundMessage);

    // scroll to the bottom
    messengerMessagingView.scrollTop = messengerMessagingView.scrollHeight;
};

socket.onclose = function(e) {
    if (e.wasClean) {
        console.log(`[close] Connection closed cleanly, code=${e.code} reason=${e.reason}`);
    } else {
        // e.g. server process killed or network down
        // event.code is usually 1006 in this case
        console.log('[close] Connection died');
    }
};

socket.onerror = function(e) {
    console.log(`[error]`);
};