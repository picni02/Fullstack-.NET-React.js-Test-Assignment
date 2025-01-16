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

