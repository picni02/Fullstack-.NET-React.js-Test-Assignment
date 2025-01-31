import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container, Form, Input, Button, Row, Col, ListGroup, ListGroupItem } from "reactstrap";


const BASE_URL = "http://localhost:5000/events";
const SEARCH_RESIDENTS_URL = "http://localhost:5000/residents/search";
const SEARCH_APARTMENTS_URL = "http://localhost:5000/apartments/search";

const EditEvent = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [event, setEvent] = useState({ eventTime: "", eventType: "Entry", residentId: "", apartmentId: "" });
    const [residentSearch, setResidentSearch] = useState("");
    const [apartmentSearch, setApartmentSearch] = useState("");
    const [residentResults, setResidentResults] = useState([]);
    const [apartmentResults, setApartmentResults] = useState([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        console.log("Event state updated:", event);
    }, [event]);    

    useEffect(() => {
        fetchEvent();
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

    const fetchEvent = async () => {
        try {
            const response = await fetch(`${BASE_URL}/${id}`);
            if (!response.ok) throw new Error("Failed to fetch event");
            const data = await response.json();
            setEvent({...data, eventType: data.eventType.toUpperCase()});
            setResidentSearch(`${data.residentId}`);
            setApartmentSearch(`${data.apartmentId}`);
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        console.log(`Changing ${name} to:`, value);
        setEvent({ ...event, [name]: name === "eventType" ? value.toUpperCase() : value });
    };

    function formatDateToCustomFormat(date) {
        const d = new Date(date);

        const year = d.getFullYear();
        const month = String(d.getMonth() + 1).padStart(2, '0'); // +1 jer mjeseci u JavaScriptu idu od 0
        const day = String(d.getDate()).padStart(2, '0');
        const hours = String(d.getHours()).padStart(2, '0');
        const minutes = String(d.getMinutes()).padStart(2, '0');
        const seconds = String(d.getSeconds()).padStart(2, '0');
        
        return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
    }

    const handleUpdate = async () => {

        const updatedEvent = { ...event, eventTime: formatDateToCustomFormat(new Date()) };
        try {
            const response = await fetch(`${BASE_URL}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedEvent),
            });
            if (!response.ok) throw new Error("Failed to update event");
            const responseText = await response.text();
            console.log("Server response:", responseText);

            navigate("/events");
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
        setEvent({ ...event, residentId: resident.residentId });
        setResidentSearch(`${resident.firstName} ${resident.lastName}`);
        setResidentResults([]); // Sakrij rezultate nakon odabira
    };

    const handleSelectApartment = (apartment) => {
        setEvent({ ...event, apartmentId: apartment.apartmentId });
        setApartmentSearch(`${apartment.apartmentNumber} - ${apartment.address}`);
        setApartmentResults([]);
    };

    return (
        <Container>
            <h1>Edit Event</h1>
            <Form className="mb-5 p-4 border rounded">
                <Row className="mb-3">
                    <Col>
                        <Input type="select" name="eventType" onChange={handleInputChange}>
                            <option value="Exit">Exit</option>
                            <option value="Entry">Entry</option>
                        </Input>
                    </Col>
                </Row>
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
                        <Button color="primary" onClick={handleUpdate}>Save</Button>
                        <Button color="secondary" className="ms-2" onClick={() => navigate("/events")}>Cancel</Button>
                    </Col>
                </Row>
                
            </Form>
        </Container>
    );
};

export default EditEvent;
