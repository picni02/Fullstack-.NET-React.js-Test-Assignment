import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import '../styles/Pages.css'
import { BASE_URL } from '../utils/config';
import { handleGenerateData } from "../services/apiService";

const Home = () => {
    const navigate = useNavigate();
    const [topBuildings, setTopBuildings] = useState([]);
    const [address, setAddress] = useState('');
    const [residents, setResidents] = useState([]);

    useEffect(() => {
        fetchTopBuildings();
    }, []);

    const fetchTopBuildings = async () => {
        try {
            const response = await fetch(`${BASE_URL}/statistics/top-buildings`);
            if (!response.ok) throw new Error('Failed to fetch top buildings');
            
            const data = await response.json();
            console.log('Fetched data:', data);
            setTopBuildings(data);
        } catch (error) {
            console.error('Error fetching top buildings:', error);
        }
    };

    const handleCheckResidentsStatus = async () => {
        if (!address.trim()) {
            alert('Please enter a valid address');
            return;
        }

        try {
            const response = await fetch(`${BASE_URL}/statistics/residents-status?address=${encodeURIComponent(address)}`);
            if (!response.ok) throw new Error('Failed to fetch residents status');

            const data = await response.json();
            console.log(data);
            setResidents(data);
        } catch (error) {
            console.error('Error fetching residents status:', error);
        }
    };

    return (
        <div className="container d-flex flex-column justify-content-center align-items-center text-center">
            <h1 className="mt-4 mb-4">Welcome to Resident Management System</h1>
            
            <div className="d-flex flex-column gap-3">
                <button className="btn btn-primary" onClick={handleGenerateData}>Generate data</button>
            </div>

            <h2 className="mt-5">Top 5 Buildings</h2>
            <ul className="list-group mt-3">
                {topBuildings.length > 0 ? (
                    topBuildings.map((building, index) => (
                        <li key={index} className="list-group-item">
                            <b>Building Address:</b> {building.address}, <b>Event Count:</b> {building.eventCount}, <b>Share:</b> {building.sharePercentage}%
                        </li>
                    ))
                ) : (
                    <li className="list-group-item">No data available</li>
                )}
            </ul>

            <div className="mt-5 w-50">
                <h3>Check Residents Status</h3>
                <input 
                    type="text" 
                    className="form-control mt-2" 
                    placeholder="Enter address" 
                    value={address} 
                    onChange={(e) => setAddress(e.target.value)}
                />
                <button className="btn btn-success mt-3" onClick={handleCheckResidentsStatus}>
                    Check Residents
                </button>
            </div>

            {residents && Object.keys(residents).length > 0 && (
                <div className="mt-4">
                    <h4>Apartments at {address}</h4>
                    <ul className="list-group">
                        <li className="list-group-item">
                            <b>Total residents:</b> {residents.totalResidents}, 
                            <b> Inside Count:</b> {residents.insideCount}, 
                            <b> Inside percentage:</b> {residents.insidePercentage}%, 
                            <b> Outside count:</b> {residents.outsideCount},
                            <b> Outside percentage:</b> {residents.outsidePercentage}%
                        </li>
                    </ul>
                </div>
            )}

        </div>
    );
};

export default Home;
