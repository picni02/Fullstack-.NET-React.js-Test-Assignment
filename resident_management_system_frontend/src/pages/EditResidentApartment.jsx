import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container, Form, Input, Button, Row, Col, ListGroup, ListGroupItem } from "reactstrap";


const BASE_URL = "http://localhost:5000/residentapartments";
const SEARCH_RESIDENTS_URL = "http://localhost:5000/residents/search";
const SEARCH_APARTMENTS_URL = "http://localhost:5000/apartments/search";

const EditResidentApartment = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [residentApartment, setResidentApartment] = useState({residentId: "", apartmentId: "" });
    const [residentSearch, setResidentSearch] = useState("");
    const [apartmentSearch, setApartmentSearch] = useState("");
    const [residentResults, setResidentResults] = useState([]);
    const [apartmentResults, setApartmentResults] = useState([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        console.log("Resident-apartment state updated:", residentApartment);
    }, [residentApartment]);    

    useEffect(() => {
        fetchResidentApartment();
    }, []);

    useEffect(() => {
        const timeout = setTimeout(() => {
            if (residentSearch.length >= 3) fetchSearchResults(residentSearch, "resident");
            else setResidentResults([]);
        }, 500);

        return () => clearTimeout(timeout);
    }, [residentSearch]);

    useEffect(() => {
        const timeout = setTimeout(() => {
            if (apartmentSearch.length >= 3) fetchSearchResults(apartmentSearch, "apartment");
            else setApartmentResults([]);
        }, 500);

        return () => clearTimeout(timeout);
    }, [apartmentSearch]);

    const fetchResidentApartment = async () => {
        try {
            const response = await fetch(`${BASE_URL}/${id}`);
            if (!response.ok) throw new Error("Failed to fetch resident-apartment.");
            const data = await response.json();
            console.log('Edit data: ', data);
            setResidentApartment({...data});
            setResidentSearch(`${data.residentId}`);
            setApartmentSearch(`${data.apartmentId}`);
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        console.log(`Changing ${name} to:`, value);
        setResidentApartment({ ...residentApartment, [name]: value });
    };

    const handleUpdate = async () => {

        try {
            const response = await fetch(`${BASE_URL}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(residentApartment),
            });
            if (!response.ok) throw new Error("Failed to update resident-apartment.");
            const responseText = await response.text();
            console.log("Server response:", responseText);

            navigate("/residentapartments");
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

    const handleSelectResident = (resident) => {
        setResidentApartment({ ...residentApartment, residentId: resident.residentId });
        setResidentSearch(`${resident.residentId} | ${resident.firstName} ${resident.lastName}`);
        setResidentResults([]); // Sakrij rezultate nakon odabira
    };

    const handleSelectApartment = (apartment) => {
        setResidentApartment({ ...residentApartment, apartmentId: apartment.apartmentId });
        setApartmentSearch(`${apartment.apartmentId} | ${apartment.apartmentNumber} - ${apartment.address}`);
        setApartmentResults([]);
    };

    return (
        <Container>
            <h1>Edit Resident-Apartment</h1>
            <Form className="mb-5 p-4 border rounded">
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
                            placeholder="Search Apartment by ID or address"
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
                        <Button color="primary" onClick={handleUpdate}>Save</Button>
                        <Button color="secondary" className="ms-2" onClick={() => navigate("/residentapartments")}>Cancel</Button>
                    </Col>
                </Row>
                
            </Form>
        </Container>
    );
};

export default EditResidentApartment;
