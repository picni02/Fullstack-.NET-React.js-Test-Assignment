import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container, Form, Input, Button, Row, Col, Label } from "reactstrap";

const BASE_URL = "http://localhost:5000/apartments";

const EditApartment = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [apartment, setApartment] = useState({ apartmentNumber: "", address: "" });

    useEffect(() => {
        fetchApartment();
    }, []);

    const fetchApartment = async () => {
        try {
            const response = await fetch(`${BASE_URL}/${id}`);
            if (!response.ok) throw new Error("Failed to fetch apartment.");
            const data = await response.json();
            setApartment(data);
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setApartment({
            ...apartment,
            [name]: type === "checkbox" ? checked : value,
        });
    };

    const handleUpdate = async () => {
        try {
            const response = await fetch(`${BASE_URL}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(apartment),
            });
            if (!response.ok) throw new Error("Failed to update apartment.");
            navigate("/apartments");
        } catch (error) {
            console.error("Error:", error);
        }
    };

    return (
        <Container>
            <h1>Edit Apartment</h1>
            <Form className="mb-5 p-4 border rounded">
                <Row className="mb-3">
                    <Col>
                        <h5>Apartment Number</h5>
                        <Input type="text" name="apartmentNumber" value={apartment.apartmentNumber} onChange={handleInputChange} />
                    </Col>
                    <Col>
                        <h5>Address</h5>
                        <Input type="text" name="address" value={apartment.address} onChange={handleInputChange} />
                    </Col>
                </Row>
                <Row>
                    <Col className="text-end">
                        <Button color="primary" onClick={handleUpdate}>Save</Button>
                        <Button color="secondary" className="ms-2" onClick={() => navigate("/apartments")}>Cancel</Button>
                    </Col>
                </Row>
                
            </Form>
        </Container>
    );
};

export default EditApartment;
