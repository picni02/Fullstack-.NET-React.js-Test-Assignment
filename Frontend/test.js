// Residents
document.getElementById("create-resident-btn").addEventListener("click", async function () {
    const response = await fetch("/residents", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ ResidentId: 3, FirstName: "Ramo", LastName: "Isak", IsInside : 1 })
    });
    const data = await response.json();
    document.getElementById("resident-output").textContent = JSON.stringify(data, null, 2);
});

document.getElementById("get-resident-btn").addEventListener("click", async function () {
    const response = await fetch("/residents/1");
    const data = await response.json();
    document.getElementById("resident-output").textContent = JSON.stringify(data, null, 2);
});

document.getElementById("delete-resident-btn").addEventListener("click", async function () {
    await fetch("/residents/2", { method: "DELETE" });
    document.getElementById("resident-output").textContent = "Resident deleted.";
});

// Events
document.getElementById("create-event-btn").addEventListener("click", async function () {
    const response = await fetch("/events", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ EventId: 3, EventTime: new Date().toISOString(), ResidentId: 3, EventType: "Entry",  ApartmentNumber : "54" })
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
