import React from "react";
import { useNavigate } from "react-router-dom";
import '../styles/Pages.css'
import {BASE_URL} from '../utils/config'

const Home = () => {
    
    const navigate = useNavigate();
    const handleGenerateData = async () => {
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

    return (
        <div className="container d-flex flex-column justify-content-center align-items-center text-center">
            <h1 className="mt-4 mb-4">Welcome to Resident Management System</h1>
            <div className="d-flex flex-column gap-3">
                <button className="btn btn-primary" onClick={() => navigate('/residents')}>Residents</button>
                <button className="btn btn-primary" onClick={() => navigate('/apartments')}>Apartments</button>
                <button className="btn btn-primary" onClick={handleGenerateData}>Generate Data</button>
            </div>
        </div>
    )
}

export default Home