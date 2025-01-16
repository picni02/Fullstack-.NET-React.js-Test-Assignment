async function getResidents() {
    try {
        const response = await fetch('http://localhost:5000/residents');
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        document.getElementById('resident-output').textContent = JSON.stringify(data, null, 2);
    } catch (error) {
        document.getElementById('resident-output').textContent = `Error: ${error.message}`;
    }
}

// Add event listener to the button
document.getElementById('get-residents-btn').addEventListener('click', getResidents);


// Residents
document.getElementById("create-resident-btn").addEventListener("click", async function () {
    const response = await fetch("/residents", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ ResidentId: 3, FirstName: 'Ramo', LastName: 'Isak', IsInside: 1 })
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
