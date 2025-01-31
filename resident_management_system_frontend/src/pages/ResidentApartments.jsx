import React, { useState, useEffect } from "react";
import { Container, Row, Col, Form, Button, Input, ListGroup, ListGroupItem } from "reactstrap";
import { useNavigate } from "react-router-dom";
import "../styles/ResidentApartment.css";

const BASE_URL = "http://localhost:5000/residentapartments";
const SEARCH_RESIDENTS_URL = "http://localhost:5000/residents/search";
const SEARCH_APARTMENTS_URL = "http://localhost:5000/apartments/search";

const ResidentApartments = () => {
    const [residentApartments, setResidentApartments] = useState([]);
    const [newResidentApartment, setNewResidentApartment] = useState({
            residentId: "",
            firstName: "",
            lastName: "",
            apartmentId: "",
            apartmentNumber: "",
            address: ""
        
    });
    const [residentSearch, setResidentSearch] = useState("");
    const [apartmentSearch, setApartmentSearch] = useState("");
    const [residentResults, setResidentResults] = useState([]);
    const [apartmentResults, setApartmentResults] = useState([]);
    const [searchQuery, setSearchQuery] = useState("");
    const [page, setPage] = useState(1);
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [hasMore, setHasMore] = useState(true);
    const [isSearching, setIsSearching] = useState(false);

    useEffect(() => {
        fetchResidentApartments(page);
    }, [page]);

    useEffect(() => {
        if (residentApartments.length >= 1000) {
            setHasMore(false);
        }
    }, [residentApartments]);

    useEffect(() => {
        const timeout = setTimeout(() => {
            if (searchQuery.length >= 3) handleSearch();
        }, 500);

        return () => clearTimeout(timeout);
    }, [searchQuery]);

    useEffect(() => {
        if (searchQuery.trim() === "") {
            setResidentApartments([]); 
            setPage(1); 
            fetchResidentApartments(1); 
        }
    }, [searchQuery]);

    useEffect(() => {
        const timeout = setTimeout(() => {
            if (residentSearch.length >= 4) fetchSearchResults(residentSearch, "resident");
            else setResidentResults([]);
        }, 500);

        return () => clearTimeout(timeout);
    }, [residentSearch]);

    useEffect(() => {
        const timeout = setTimeout(() => {
            if (apartmentSearch.length >= 4) fetchSearchResults(apartmentSearch, "apartment");
            else setApartmentResults([]);
        }, 500);

        return () => clearTimeout(timeout);
    }, [apartmentSearch]);

    const fetchResidentApartments = async (newPage = 1) => {
        try {
            const response = await fetch(`${BASE_URL}?page=${newPage}&pageSize=20`);
            if (!response.ok) throw new Error("Failed to fetch resident-apartments.");
    
            const data = await response.json();
            console.log(data)
            setResidentApartments(prev => {
                const newResidentApartments = data.map(resAp => {
                    if (!resAp.residentApartmentId) {
                        console.warn("Missing resident-apartment data:", resAp);
                        return null; 
                    }
    
                    return {
                            residentApartmentId: resAp.residentApartmentId,
                            residentId: resAp.residentId,
                            firstName: resAp.firstName,
                            lastName: resAp.lastName,
                            apartmentId: resAp.apartmentId,
                            apartmentNumber: resAp.apartmentNumber,
                            address: resAp.address,
                        
                    };
                }).filter(item => item !== null)
                .filter(newResAp => 
                    !prev.some(existingResAp => 
                        existingResAp.residentId === newResAp.residentId &&
                        existingResAp.apartmentId === newResAp.apartmentId
                    )
                );
    
                return [...prev, ...newResidentApartments];
            });
            
            setPage(newPage);
            setHasMore(data.length === 20)
            if (data.length < 10) {
                setHasMore(false);
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };


    const fetchSearchResults = async (query, type) => {
        setLoading(true);
        try {
            const response = await fetch(
                type === "resident"
                    ? `${SEARCH_RESIDENTS_URL}?query=${encodeURIComponent(query)}`
                    : `${SEARCH_APARTMENTS_URL}?query=${encodeURIComponent(query)}`
            );

            if (!response.ok) throw new Error("Failed to fetch search results");
            const data = await response.json();

            if (type === "resident") setResidentResults(data);
            else setApartmentResults(data);
        } catch (error) {
            console.error("Error:", error);
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = async (newPage = 1) => {
        if (!searchQuery.trim()) {
            alert("Please enter a search term.");
            setResidentApartments([]);
            setPage(1);
            fetchResidentApartments(1);
            return;
        }
        
        setIsSearching(true);

        try {
            const response = await fetch(`${BASE_URL}/search?query=${encodeURIComponent(searchQuery)}&page=${newPage}&pageSize=20}`);
            if (response.status === 404) {
                if(newPage === 1){
                    alert("No relations found.");
                    setResidentApartments([]);
                }
                setHasMore(false);
                return;
            }
            if (!response.ok) throw new Error("Failed to search relations.");
            const result = await response.json();
            if(newPage === 1){
                setResidentApartments(result.results);
            }else{
                setResidentApartments([...residentApartments, ...result.results]);
            }

            setPage(newPage);
            setHasMore(result.results.length === 20);
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleSelectResident = (resident) => {
        setNewResidentApartment({ ...newResidentApartment, residentId: resident.residentId });
        setResidentSearch(`${resident.firstName} ${resident.lastName}`);
        setResidentResults([]); 
    };

    const handleSelectApartment = (apartment) => {
        setNewResidentApartment({ ...newResidentApartment, apartmentId: apartment.apartmentId });
        setApartmentSearch(`${apartment.apartmentNumber} - ${apartment.address}`);
        setApartmentResults([]);
    };

    const handleAddResidentApartment = async () => {
        if (!newResidentApartment.residentId || !newResidentApartment.apartmentId) {
            alert("All fields are required!");
            return;
        }
        console.log(newResidentApartment.apartmentId, newResidentApartment.residentId);
        try {
            const response = await fetch(`${BASE_URL}/`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newResidentApartment),
            });

            if(response.status === 409)
            {
                const errorResponse = await response.json();
                alert(`Resident ${newResidentApartment.residentId} is already assigned to apartment ${newResidentApartment.apartmentId}!`);
                return;
            }
            if (!response.ok){
                throw new Error("Failed to add resident-apartment.");
            }

            let addedResidentApartment = null;

            if(response.status !== 204){
                addedResidentApartment = await response.json();
            }   
            setResidentApartments([addedResidentApartment, ...residentApartments]);
           
            setNewResidentApartment({ resident: {
                residentId: "",
                firstName: "",
                lastName: ""
            },
            apartment: {
                apartmentId: "",
                apartmentNumber: "",
                address: ""
            }  });
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleDelete = async (residentApartmentsId) => {
        if (!window.confirm("Are you sure you want to delete this resident-apartment?")) return;
        if (!residentApartmentsId) {
            console.error("Invalid residentApartmentId!");
            return;
        }
        try {
            const response = await fetch(`${BASE_URL}/${residentApartmentsId}`, { method: "DELETE" });
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            if (response.status === 204) {
                setResidentApartments(residentApartments.filter((ra) => ra.residentApartmentId !== residentApartmentsId));
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleEdit = (id) => {
        navigate(`/edit-resident-apartment/${id}`);
    };

    return (
        <Container>
            <Row>
                <Col>
                    <h1 className="mt-4">Resident-Apartments Relations</h1>
                    <Form className="mb-4 p-3 border rounded">
                        <Row className="mb-3">
                            <Col>
                                <Input
                                    type="text"
                                    placeholder="Search Resident by ID or Name"
                                    value={residentSearch}
                                    onChange={(e) => setResidentSearch(e.target.value)}
                                />
                                {loading && <p>Loading...</p>}
                                {residentResults.length > 0 && (
                                    <ListGroup className="search-results">
                                        {residentResults.map((res) => (
                                            <ListGroupItem
                                                key={res.residentId}
                                                action
                                                onClick={() => handleSelectResident(res)}
                                            >
                                                {res.residentId} - {res.firstName} {res.lastName}
                                            </ListGroupItem>
                                        ))}
                                    </ListGroup>
                                )}
                            </Col>
                        </Row>

                        <Row className="mb-3">
                            <Col>
                                <Input
                                    type="text"
                                    placeholder="Search Apartment by ID"
                                    value={apartmentSearch}
                                    onChange={(e) => setApartmentSearch(e.target.value)}
                                />
                                {loading && <p>Loading...</p>}
                                {apartmentResults.length > 0 && (
                                    <ListGroup className="search-results">
                                        {apartmentResults.map((apt) => (
                                            <ListGroupItem
                                                key={apt.apartmentId}
                                                action
                                                onClick={() => handleSelectApartment(apt)}
                                            >
                                                {apt.apartmentId} - {apt.apartmentNumber} ({apt.address})
                                            </ListGroupItem>
                                        ))}
                                    </ListGroup>
                                )}
                            </Col>
                        </Row>

                        <Row>
                            <Col className="text-end">
                                <Button color="success" onClick={handleAddResidentApartment}>Add Relation</Button>
                            </Col>
                        </Row>
                    </Form>

                    <Form className="mb-4 p-3 border rounded">
                        <Row>
                            <Col>
                                <Input type="text" placeholder="Search by ID, resident ID, apartment ID..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} />
                            </Col>
                            <Col className="text-end">
                                <Button color="primary" onClick={handleSearch}>Search</Button>
                            </Col>
                        </Row>
                    </Form>

                    <ListGroup className="mt-4">
                        {residentApartments.map((ra, index) => (
                            <ListGroupItem key={index} className="d-flex justify-content-between align-items-center p-3">
                                <div>
                                    <h6><b>ID:</b> {ra.residentApartmentId} </h6>
                                    <h6><b>Resident ID:</b> {ra.residentId}</h6>
                                    <h6><b>Resident full name:</b> {ra.firstName} {ra.lastName}</h6>
                                    <h6><b>Apartment ID:</b> {ra.apartmentId}</h6>
                                    <h6><b>Apartment number and address:</b> {ra.apartmentNumber} {ra.address}</h6>
                                </div>
                                <div>
                                    <Button color="warning" className="me-2" onClick={() => handleEdit(ra.residentApartmentId)}>Edit</Button>
                                    <Button color="danger" onClick={() => handleDelete(ra.residentApartmentId)}>Delete</Button>
                                </div>
                            </ListGroupItem>
                        ))}
                    </ListGroup>
                    {hasMore && residentApartments.length > 0 && (
                        <Button 
                            color="secondary" 
                            className="mt-3" 
                            onClick={() => isSearching ? handleSearch(page + 1) : fetchResidentApartments(page + 1)}
                        >
                            Load More
                        </Button>
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default ResidentApartments;
