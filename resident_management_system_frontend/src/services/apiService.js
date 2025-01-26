import { BASE_URL } from '../utils/config';

// Funkcija za generisanje podataka
export const generateData = async () => {
    try {
        const response = await fetch(`${BASE_URL}/generate-data`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (!response.ok) {
            throw new Error('Server returned an error response');
        }

        const result = await response.json();

        if (result.error) {
            throw new Error(result.error);
        }

        alert(result.message || 'Data generated successfully!');
    } catch (error) {
        console.error('Error generating data:', error);
        alert('Failed to generate data. Please try again.');
    }
};

// Funkcija za dohvaćanje statusa rezidenata na određenoj adresi
export const fetchResidentStatus = async (address) => {
    if (!address.trim()) {
        alert('Address cannot be empty!');
        return;
    }

    try {
        const response = await fetch(`${BASE_URL}/api/statistics/residents-status?address=${encodeURIComponent(address)}`);

        if (!response.ok) {
            throw new Error('Failed to fetch data');
        }

        const data = await response.json();
        alert(`Total: ${data.totalResidents}, Inside: ${data.insideCount}, Outside: ${data.outsideCount}`);
    } catch (error) {
        console.error('Error fetching resident status:', error);
        alert('Error fetching resident status. Please try again.');
    }
};

// Funkcija za dohvaćanje top 5 zgrada sa najviše događaja
export const fetchTopBuildings = async () => {
    try {
        const response = await fetch(`${BASE_URL}/api/statistics/top-buildings`);

        if (!response.ok) {
            throw new Error('Failed to fetch data');
        }

        const data = await response.json();
        console.log('Top buildings:', data);

        let resultMessage = "Top 5 Buildings:\n";
        data.forEach((building, index) => {
            resultMessage += `${index + 1}. Address: ${building.Address}, Events: ${building.EventCount}, Share: ${building.SharePercentage}%\n`;
        });
        
        alert(resultMessage);
    } catch (error) {
        console.error('Error fetching top buildings:', error);
        alert('Error fetching top buildings. Please try again.');
    }
};

export const handleGenerateData = async () => {
    try{
        const response = await fetch(`${BASE_URL}/generate-data`, { 
            method: 'POST', 
            headers: { 'Content-Type' : 'application/json' }
        });
        if(!response.ok){
            throw new Error('Server returned an error response');
        }

        const result = await response.json();

        if(result.error){
            throw new Error(result.error);
        }
        
        alert(result.message || 'Data generated successfully!');
        
    }catch(error)
    {
        console.error('Error generating data:', error);
        alert('Failed to generate data. Please try again.');
    }
    
};