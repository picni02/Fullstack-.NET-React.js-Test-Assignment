import React, { useState, useEffect } from "react";
import { Container, Row, Col, Form, Button, Input, ListGroup, ListGroupItem, Label } from "reactstrap";
import { useNavigate } from "react-router-dom";
import "../styles/Apartment.css"

const BASE_URL = "http://localhost:5000/apartments";

const Apartments = () => {
    const [apartments, setApartments] = useState([]);
    const [newApartment, setNewApartment] = useState({ apartmentNumber: "", address: ""});
    const [searchQuery, setSearchQuery] = useState("");
    const [page, setPage] = useState(1);
    const navigate = useNavigate();
    const [hasMore, setHasMore] = useState(true);

    useEffect(() => {
        fetchApartments(page);
    }, [page]);

    useEffect(() => {
        if (apartments.length >= 1000) {
            setHasMore(false);
        }
    }, [apartments]);
    
    useEffect(() => {
            const timeout = setTimeout(() => {
                if (searchQuery.length >= 3) handleSearch();
            }, 500);
    
            return () => clearTimeout(timeout);
        }, [searchQuery]);

    useEffect(() => {
        if (searchQuery.trim() === "") {
            setApartments([]); // Resetuj listu
            setPage(1); // Resetuj paginaciju
            fetchApartments(1); // Učitaj početne podatke
        }
    }, [searchQuery]);
        

    const fetchApartments = async (pageNumber) => {
        try {
            const response = await fetch(`${BASE_URL}?page=${pageNumber}&pageSize=20`);
            if (!response.ok) throw new Error("Failed to fetch apartments!");
    
            const data = await response.json();

            setApartments(prev => {
                const newApartments = data.filter(apartment => 
                    !prev.some(a => a.apartmentId === apartment.apartmentId)
                );
                return [...prev, ...newApartments];
            });
    
            // Sakrij dugme ako je manje rezultata nego što je pageSize
            if (data.length < 10) {
                setHasMore(false);
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };
    

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setNewApartment({
            ...newApartment,
            [name]: type === "checkbox" ? checked : value,
        });
    };

    const handleAddApartment = async () => {
        if (!newApartment.apartmentNumber || !newApartment.address) {
            alert("All fields are required!");
            return;
        }

        const payload = {
            apartmentNumber: newApartment.apartmentNumber,
            address: newApartment.address
        }

        try {
            const response = await fetch(BASE_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload),
            });
            if (!response.ok) throw new Error("Failed to add apartment!");
            const addedApartment = await response.json();
            setApartments([addedApartment, ...apartments]);
            setNewApartment({ apartmentNumber: "", address: "" });
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleSearch = async () => {
        if (!searchQuery.trim()) {
            alert("Please enter a search term.");
            setApartments([]);
            setPage(1);
            fetchApartments(1);
            return;
        }
    
        try {
            const response = await fetch(`${BASE_URL}/search?query=${encodeURIComponent(searchQuery)}`);
            if (response.status === 404) {
                alert("No apartments found.");
                return;
            }
            if (!response.ok) throw new Error("Failed to search apartments");
            const result = await response.json();
            setApartments(result);
        } catch (error) {
            console.error("Error:", error);
        }
    };
    
    
    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this apartment?")) return;
        try {
            const response = await fetch(`${BASE_URL}/${id}`, { method: "DELETE" });
            if (response.status === 204) {
                setApartments(apartments.filter((apa) => apa.apartmentId !== id));
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleEdit = (id) => {
        navigate(`/edit-apartment/${id}`);
    };

    return (
        <Container>
            <Row>
                <Col>
                    <h1 className="mt-4">Apartments</h1>
                    <Form className="mb-4 p-3 border rounded">
                        <Row className="mb-3">
                            <Col>
                                <Input type="text" name="apartmentNumber" placeholder="Apartment number" value={newApartment.apartmentNumber} onChange={handleInputChange} />
                            </Col>
                            <Col>
                                <Input type="text" name="address" placeholder="Address" value={newApartment.address} onChange={handleInputChange} />
                            </Col>
                        </Row>
                        <Row>
                            <Col className="text-end">
                                <Button color="success" onClick={handleAddApartment}>Add Apartment</Button>
                            </Col>
                        </Row>
                    </Form>

                    <Form className="mb-4 p-3 border rounded">
                        <Row>
                            <Col>
                                <Input type="text" placeholder="Search by ID, number, address..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} />
                            </Col>
                            <Col className="text-end">
                                <Button color="primary" onClick={handleSearch}>Search</Button>
                            </Col>
                        </Row>
                    </Form>

                    <ListGroup className="mt-4">
                        {[...apartments]
                          //  .sort((a, b) => b.apartmentId - a.apartmentId)
                            .map((apartment, index) => (
                                <ListGroupItem key={apartment.ApartmentId || index} className="d-flex justify-content-between align-items-center p-3">
                                    <div>
                                        <h5><b>Apartment ID:</b> {apartment.apartmentId} </h5>
                                        <h6><b>Apartment Number:</b> {apartment.apartmentNumber}</h6>
                                        <h6><b>Address:</b> {apartment.address}</h6>
                                    </div>
                                    <div>
                                        <Button color="warning" className="me-2" onClick={() => handleEdit(apartment.apartmentId)}>Edit</Button>
                                        <Button color="danger" onClick={() => handleDelete(apartment.apartmentId)}>Delete</Button>
                                    </div>
                                </ListGroupItem>
                        ))}
                    </ListGroup>
                    {hasMore && apartments.length > 0 && (
                        <Button color="secondary" className="mt-3" onClick={() => setPage(page + 1)}>Load More</Button>
                    )}

                </Col>
            </Row>
        </Container>
    );
};

export default Apartments;
