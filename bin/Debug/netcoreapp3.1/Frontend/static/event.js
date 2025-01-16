// Events
document.getElementById("create-event-btn").addEventListener("click", async function () {
    const response = await fetch("/events", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ EventId: 3, EventTime: new Date().toISOString(), ResidentId: 3, EventType: "Entry", ApartmentNumber: "54" })
    });
    const data = await response.json();
    document.getElementById("event-output").textContent = JSON.stringify(data, null, 2);
});

document.getElementById("get-events-btn").addEventListener("click", async function () {
    const response = await fetch("/events/1");
    const data = await response.json();
    document.getElementById("event-output").textContent = JSON.stringify(data, null, 2);
});

document.getElementById("delete-event-btn").addEventListener("click", async function () {
    await fetch("/events/2", { method: "DELETE" });
    document.getElementById("event-output").textContent = "Event deleted.";
});