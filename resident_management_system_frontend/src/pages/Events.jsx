import React, { useState, useEffect } from "react";
import { Container, Row, Col, Form, Button, Input, ListGroup, ListGroupItem } from "reactstrap";
import { useNavigate } from "react-router-dom";
import "../styles/Event.css";

const BASE_URL = "http://localhost:5000/events";
const SEARCH_RESIDENTS_URL = "http://localhost:5000/residents/search";
const SEARCH_APARTMENTS_URL = "http://localhost:5000/apartments/search";

const Events = () => {
    const [events, setEvents] = useState([]);
    const [newEvent, setNewEvent] = useState({ eventTime: formatDateToCustomFormat(new Date()), eventType: "Entry", residentId: "", apartmentId: "" });
    const [residentSearch, setResidentSearch] = useState("");
    const [apartmentSearch, setApartmentSearch] = useState("");
    const [residentResults, setResidentResults] = useState([]);
    const [apartmentResults, setApartmentResults] = useState([]);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const [hasMore, setHasMore] = useState(true);

    useEffect(() => {
        fetchEvents(page);
    }, [page]);

    useEffect(() => {
        if (events.length >= 1000) {
            setHasMore(false);
        }
    }, [events]);

    // Debounce logika (čekaj 500ms prije slanja zahtjeva)
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

    const fetchEvents = async (pageNumber) => {
        try {
            const response = await fetch(`${BASE_URL}?page=${pageNumber}&pageSize=10`);
            if (!response.ok) throw new Error("Failed to fetch events");
    
            const data = await response.json();
    
            setEvents(prev => {
                const newEvents = data.filter(even => 
                    !prev.some(ev => ev.eventId === even.eventId)
                );
                return [...prev, ...newEvents];
            });
    
            // Sakrij dugme ako je manje rezultata nego što je pageSize
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

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        if(name === "eventTime"){
            setNewEvent({ ...newEvent, [name]: formatDateToCustomFormat(value) });
        }else{
            setNewEvent({ ...newEvent, [name]: value });
        }
    };

    const handleSelectResident = (resident) => {
        setNewEvent({ ...newEvent, residentId: resident.residentId });
        setResidentSearch(`${resident.firstName} ${resident.lastName}`);
        setResidentResults([]); // Sakrij rezultate nakon odabira
    };

    const handleSelectApartment = (apartment) => {
        setNewEvent({ ...newEvent, apartmentId: apartment.apartmentId });
        setApartmentSearch(`${apartment.apartmentNumber} - ${apartment.address}`);
        setApartmentResults([]);
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

    const handleAddEvent = async () => {
        if (!newEvent.eventTime || !newEvent.residentId || !newEvent.apartmentId) {
            alert("All fields are required!");
            return;
        }

        const updatedEvent = { ...newEvent, eventTime: formatDateToCustomFormat(new Date()) };

        try {
            const response = await fetch(BASE_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedEvent),
            });

            if(response.status === 401)
            {
                const errorResponse = await response.json();
                alert(`Resident ${newEvent.residentId} doesn't have access to apartment ${newEvent.apartmentId}!`);
                return;
            }
            if (!response.ok){
                throw new Error("Failed to add event");
            }
                
            const addedEvent = await response.json();
            setEvents([addedEvent, ...events]);
            setNewEvent({ eventTime: formatDateToCustomFormat(new Date()), eventType: "Entry", residentId: "", apartmentId: "" });
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this event?")) return;
        try {
            const response = await fetch(`${BASE_URL}/${id}`, { method: "DELETE" });
            if (response.status === 204) {
                setEvents(events.filter((ev) => ev.eventId !== id));
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleEdit = (id) => {
        navigate(`/edit-event/${id}`);
    };

    return (
        <Container>
            <Row>
                <Col>
                    <h1>Events</h1>
                    <Form className="mb-4 p-3 border rounded">
                        <Row className="mb-3">    
                            <Col>
                                <Input type="select" name="eventType" value={newEvent.eventType} onChange={handleInputChange}>
                                    <option value="Entry">Entry</option>
                                    <option value="Exit">Exit</option>
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
                                <Button color="success" onClick={handleAddEvent}>Add Event</Button>
                            </Col>
                        </Row>
                    </Form>

                    <ListGroup className="mt-4">
                        {events.map((event, index) => (
                            <ListGroupItem key={event.eventId || index} className="d-flex justify-content-between align-items-center p-3">
                                <div>
                                    <h5><b>ID:</b> {event.eventId}</h5>
                                    <h6><b>Resident ID:</b> {event.residentId}</h6>
                                    <h6><b>Apartment ID:</b> {event.apartmentId}</h6>
                                    <p className="mb-0"><b>Time:</b> {new Date(event.eventTime).toLocaleString("en-GB", { timeZone: "Europe/Zagreb"})}</p>
                                    <p className="mb-0"><b>Type:</b> {event.eventType}</p>
                                </div>
                                <div>
                                    <Button color="warning" className="me-2" onClick={() => handleEdit(event.eventId)}>Edit</Button>
                                    <Button color="danger" onClick={() => handleDelete(event.eventId)}>Delete</Button>
                                </div>
                            </ListGroupItem>
                        ))}
                    </ListGroup>
                    {hasMore && events.length > 0 && (
                        <Button color="secondary" className="mt-3" onClick={() => setPage(page + 1)}>Load More</Button>
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default Events;
