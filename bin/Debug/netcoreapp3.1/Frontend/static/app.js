// Script for handling button actions
document.getElementById("transfer-to-elasticsearch-btn").addEventListener("click", function () {
    fetch('/api/data/transfer', { method: 'POST' })
        .then(response => response.json())
        .then(data => {
            document.getElementById("output").textContent = JSON.stringify(data, null, 2);
        })
        .catch(error => {
            document.getElementById("output").textContent = "Error: " + error.message;
        });
});

document.getElementById("generate-data-btn").addEventListener("click", function () {
    fetch('/api/data/generate', { method: 'POST' })
        .then(response => response.json())
        .then(data => {
            document.getElementById("output").textContent = JSON.stringify(data, null, 2);
        })
        .catch(error => {
            document.getElementById("output").textContent = "Error: " + error.message;
        });
});

document.getElementById("generateDataButton").addEventListener("click", async function () {
    try {
        const response = await fetch("http://localhost:5000/generate-data", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            }
        });

        const data = await response.json();
        if (response.ok) {
            alert(data.message);
        } else {
            alert(`Error: ${data.error}`);
        }
    } catch (error) {
        alert("An error occurred: " + error.message);
    }
});
