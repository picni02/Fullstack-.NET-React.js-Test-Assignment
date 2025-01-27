import { BASE_URL } from '../utils/config';


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

export const handleTransferData = async () => {
    try{
        const response = await fetch(`${BASE_URL}/transfer-data`, { 
            method: 'GET', 
            headers: { 'Content-Type' : 'application/json' }
        });
        if(!response.ok){
            throw new Error('Server returned an error response');
        }

        const result = await response.json();

        if(result.error){
            throw new Error(result.error);
        }
        
        alert(result.message || 'Data transferred successfully!');
        
    }catch(error)
    {
        console.error('Error transferring data:', error);
        alert('Failed to transfer data. Please try again.');
    }
    
};