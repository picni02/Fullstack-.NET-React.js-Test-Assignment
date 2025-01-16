async function getResidents() {
    try {
        const response = await fetch('http://localhost:5000/residents');
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        document.getElementById('output').textContent = JSON.stringify(data, null, 2);
    } catch (error) {
        document.getElementById('output').textContent = `Error: ${error.message}`;
    }
}

// Add event listener to the button
document.getElementById('get-residents-btn').addEventListener('click', getResidents);
