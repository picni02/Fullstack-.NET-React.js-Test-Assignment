import React, { useState, useEffect } from "react";
import { Row, Col, Card } from "reactstrap";
import '../styles/Home.css'
import { BASE_URL } from '../utils/config';

const Home = () => {
    const [topBuildings, setTopBuildings] = useState([]);
    const [address, setAddress] = useState('');
    const [residents, setResidents] = useState([]);
    const [nextTransferTime, setNextTransferTime] = useState(null);
    const [isLoading, setIsLoading] = useState(false);
    const [activeButton, setActiveButton] = useState(null);

    useEffect(() => {
        fetchNextTransferTime();
    }, []);

    useEffect(() => {
        fetchTopBuildings();
    }, []);

    useEffect(() => {
        if (!address.trim()) {
            setResidents(null);
        }
    }, [address]);
    
    const fetchNextTransferTime = async () => {
        try {
            const response = await fetch(`${BASE_URL}/transfer-data`);
            if (!response.ok) throw new Error("Failed to fetch transfer time!");

            const data = await response.json();
            console.log(data)
            setNextTransferTime(new Date(data).toLocaleString("en-GB", { timeZone: "Europe/Zagreb"}));
        } catch (error) {
            console.error("Error fetching transfer time:", error);
        }
    };

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

        setResidents(null);

        try {
            const response = await fetch(`${BASE_URL}/statistics/residents-status?address=${encodeURIComponent(address)}`);
            
            if(response.status === 404){
                const errorData = await response.json();
                setResidents(errorData.error || "No residents found for this address");
                return;
            }

            if (!response.ok) throw new Error('Failed to fetch residents status');
            
            const data = await response.json();
            console.log(data);

            if (!data || Object.keys(data).length === 0) {
                setResidents("No residents found for this address"); 
            } else {
                setResidents(data);
            }
        } catch (error) {
            console.error('Error fetching residents status:', error);
            setResidents("Error fetching residents status");
        }
    };

    const handleTransferData = async () => {
        setIsLoading(true);
        setActiveButton("transfer");

        try{
            const response = await fetch(`${BASE_URL}/transfer-data`, { 
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
            
        }catch(error)
        {
            console.error('Error transferring data:', error);
            alert('Failed to transfer data. Please try again.');
        }finally{
            setIsLoading(false);
            setActiveButton(null);
            setTimeout(() => {
                alert('Data transfer completed successfully!');
            }, 500);
        }
        
    };

    const handleGenerateData = async () => {
        setIsLoading(true);
        setActiveButton("generate");
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
            
        }catch(error)
        {
            console.error('Error generating data:', error);
            alert('Failed to generate data. Please try again.');
        }finally{
            setIsLoading(false);
            setActiveButton(null);
            setTimeout(() => {
                alert('Data generated successfully!');
            }, 300);
        }
        
    };

    return (
        <div className="container d-flex flex-column justify-content-center align-items-center text-center">
            <h1 className="mt-4 mb-4">Welcome to Resident Management System</h1>
            <p>Data is usually transferred automatically every Monday at 0:00AM.</p>
            {nextTransferTime ? (
                <p>Next data transfer scheduled for: <strong>{nextTransferTime}</strong></p>
            ) : (
                <p>Loading next transfer time...</p>
            )}
            
            <div className="d-flex flex-column gap-3 mb-4 pt-2">
                <button className="btn btn-primary" onClick={handleGenerateData} disabled={isLoading}>{isLoading && activeButton === "generate" ? 'Generating data...' : 'Generate data'}</button>
                <button className="btn btn-primary" onClick={handleTransferData} disabled={isLoading}>{isLoading && activeButton === "transfer" ? 'Transferring data...' : 'Transfer data'}</button>
                {isLoading && (
                    <div className="loading-indicator">
                        <div className="spinner"></div> 
                        <p className="mt-2">Processing, please wait...</p>
                    </div>
                )}
            </div>
            
            <h2 className="mt-5">Top 5 Buildings</h2>
            <Row className="mt-3">
                {topBuildings.length > 0 ? (
                    topBuildings.map((building, index) => (
                        <Col key={index} md="4" className="mb-4">
                            <Card className="p-3 shadow-sm">
                                <h5>Building Address</h5>
                                <p>{building.address}</p>
                                <hr />
                                <p><strong>Event Count:</strong> {building.eventCount}</p>
                                <p><strong>Share:</strong> {building.sharePercentage}%</p>
                            </Card>
                        </Col>
                    ))
                ) : (
                    <Col>
                        <Card className="p-3 shadow-sm">
                            <p>No data available</p>
                        </Card>
                    </Col>
                )}
            </Row>

            <div className="mt-5 w-50">
                <h3>Check Residents Status</h3>
                <input 
                    type="text" 
                    className="form-control mt-2" 
                    placeholder="Enter address" 
                    value={address} 
                    onChange={(e) => setAddress(e.target.value)}
                />
                <button className="btn btn-success mt-3 mb-3" onClick={handleCheckResidentsStatus} disabled={isLoading}>
                    Check Residents
                </button>
            </div>

            {residents && typeof residents === "string" ? (
                <div className="mt-4">
                    <h4>{residents}</h4>
                </div>
            ) : (
                residents && Object.keys(residents).length > 0 && (
                    <div className="mt-4">
                        <h4>Apartments at {address}</h4>
                        <Row className="mt-3">
                            <Col>
                                <Card className="p-3 shadow-sm">
                                    <h5>Total Residents</h5>
                                    <p>{residents.totalResidents}</p>
                                </Card>
                            </Col>
                            <Col>
                                <Card className="p-3 shadow-sm">
                                    <h5>Inside Count</h5>
                                    <p>{residents.insideCount}</p>
                                </Card>
                            </Col>
                            <Col>
                                <Card className="p-3 shadow-sm">
                                    <h5>Inside Percentage</h5>
                                    <p>{residents.insidePercentage}%</p>
                                </Card>
                            </Col>
                        </Row>
                        <Row className="mt-3 mb-3">
                            <Col>
                                <Card className="p-3 shadow-sm">
                                    <h5>Outside Count</h5>
                                    <p>{residents.outsideCount}</p>
                                </Card>
                            </Col>
                            <Col>
                                <Card className="p-3 shadow-sm">
                                    <h5>Outside Percentage</h5>
                                    <p>{residents.outsidePercentage}%</p>
                                </Card>
                            </Col>
                        </Row>
                    </div>
                )
            )}

        </div>
    );
};

export default Home;
