import React, { useState, useEffect } from "react";
import { Container, Row, Col, Form, Button, Input, ListGroup, ListGroupItem, Label } from "reactstrap";
import { useNavigate } from "react-router-dom";
import "../styles/Resident.css"

const BASE_URL = "http://localhost:5000/residents";

const Residents = () => {
    const [residents, setResidents] = useState([]);
    const [newResident, setNewResident] = useState({ firstName: "", lastName: "", isInside: false });
    const [searchQuery, setSearchQuery] = useState("");
    const [page, setPage] = useState(1);
    const navigate = useNavigate();
    const [hasMore, setHasMore] = useState(true);

    useEffect(() => {
        fetchResidents(page);
    }, [page]);

    useEffect(() => {
        if (residents.length >= 1000) {
            setHasMore(false);
        }
    }, [residents]);
    
    useEffect(() => {
        const timeout = setTimeout(() => {
            if (searchQuery.length >= 3) handleSearch();
        }, 500);

        return () => clearTimeout(timeout);
    }, [searchQuery]);

    useEffect(() => {
        if (searchQuery.trim() === "") {
            setResidents([]); // Resetuj listu
            setPage(1); // Resetuj paginaciju
            fetchResidents(1); // Učitaj početne podatke
        }
    }, [searchQuery]);

    const fetchResidents = async (pageNumber) => {
        try {
            const response = await fetch(`${BASE_URL}?page=${pageNumber}&pageSize=10`);
            if (!response.ok) throw new Error("Failed to fetch residents");
    
            const data = await response.json();
    
            setResidents(prev => {
                const newResidents = data.filter(resident => 
                    !prev.some(r => r.residentId === resident.residentId)
                );
                return [...prev, ...newResidents];
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
        setNewResident({
            ...newResident,
            [name]: type === "checkbox" ? checked : value,
        });
    };

    const handleAddResident = async () => {
        if (!newResident.firstName || !newResident.lastName) {
            alert("All fields are required!");
            return;
        }

        const payload = {
            firstName: newResident.firstName,
            lastName: newResident.lastName,
            isInside: newResident.isInside ? true : false
        }

        try {
            const response = await fetch(BASE_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload),
            });
            if (!response.ok) throw new Error("Failed to add resident");
            const addedResident = await response.json();
            setResidents([addedResident, ...residents]);
            setNewResident({ firstName: "", lastName: "", isInside: false });
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleSearch = async () => {
        if (!searchQuery.trim()) {
            alert("Please enter a search term.");
            setResidents([]);
            setPage(1);
            fetchResidents(1);
            return;
        }
    
        try {
            const response = await fetch(`${BASE_URL}/search?query=${encodeURIComponent(searchQuery)}`);
            if (response.status === 404) {
                alert("No residents found.");
                return;
            }
            if (!response.ok) throw new Error("Failed to search residents");
            const result = await response.json();
            setResidents(result);
        } catch (error) {
            console.error("Error:", error);
        }
    };
    
    
    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this resident?")) return;
        try {
            const response = await fetch(`${BASE_URL}/${id}`, { method: "DELETE" });
            if (response.status === 204) {
                setResidents(residents.filter((res) => res.residentId !== id));
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleEdit = (id) => {
        navigate(`/edit-resident/${id}`);
    };

    return (
        <Container>
            <Row>
                <Col>
                    <h1>Residents</h1>
                    <Form className="mb-4 p-3 border rounded">
                        <Row className="mb-3">
                            <Col>
                                <Input type="text" name="firstName" placeholder="First Name" value={newResident.firstName} onChange={handleInputChange} />
                            </Col>
                            <Col>
                                <Input type="text" name="lastName" placeholder="Last Name" value={newResident.lastName} onChange={handleInputChange} />
                            </Col>
                        </Row>
                        <Row>
                            <Col>
                                <Label>
                                    <Input className="mx-2" type="checkbox" name="isInside" checked={newResident.isInside} onChange={handleInputChange} />
                                    Inside
                                </Label>
                            </Col>
                            <Col className="text-end">
                                <Button color="success" onClick={handleAddResident}>Add Resident</Button>
                            </Col>
                        </Row>
                    </Form>

                    <Form className="mb-4 p-3 border rounded">
                        <Row>
                            <Col>
                                <Input type="text" placeholder="Search by name, ID..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} />
                            </Col>
                            <Col className="text-end">
                                <Button color="primary" onClick={handleSearch}>Search</Button>
                            </Col>
                        </Row>
                    </Form>

                    <ListGroup className="mt-4">
                        {[...residents]
                         //   .sort((a, b) => b.residentId - a.residentId)
                            .map((resident, index) => (
                                <ListGroupItem key={resident.residentId || index} className="d-flex justify-content-between align-items-center p-3">
                                    <div>
                                        <h5><b>ID:</b> {resident.residentId}</h5>
                                        <h6><b>Full name:</b> {resident.firstName} {resident.lastName}</h6>
                                        <p className="mb-0"><b>Status:</b> {resident.isInside ? "Inside" : "Outside"}</p>
                                    </div>
                                    <div>
                                        <Button color="warning" className="me-2" onClick={() => handleEdit(resident.residentId)}>Edit</Button>
                                        <Button color="danger" onClick={() => handleDelete(resident.residentId)}>Delete</Button>
                                    </div>
                                </ListGroupItem>
                        ))}
                    </ListGroup>
                    {hasMore && residents.length > 0 && (
                        <Button color="secondary" className="mt-3" onClick={() => setPage(page + 1)}>Load More</Button>
                    )}

                </Col>
            </Row>
        </Container>
    );
};

export default Residents;
