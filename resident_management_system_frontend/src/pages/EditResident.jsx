import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container, Form, Input, Button, Row, Col, Label } from "reactstrap";

const BASE_URL = "http://localhost:5000/residents";

const EditResident = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [resident, setResident] = useState({ firstName: "", lastName: "", isInside: false });

    useEffect(() => {
        fetchResident();
    }, []);

    const fetchResident = async () => {
        try {
            const response = await fetch(`${BASE_URL}/${id}`);
            if (!response.ok) throw new Error("Failed to fetch resident");
            const data = await response.json();
            setResident(data);
        } catch (error) {
            console.error("Error:", error);
        }
    };

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setResident({
            ...resident,
            [name]: type === "checkbox" ? checked : value,
        });
    };

    const handleUpdate = async () => {
        try {
            const response = await fetch(`${BASE_URL}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(resident),
            });
            if (!response.ok) throw new Error("Failed to update resident");
            navigate("/residents");
        } catch (error) {
            console.error("Error:", error);
        }
    };

    return (
        <Container>
            <h1>Edit Resident</h1>
            <Form className="mb-5 p-4 border rounded">
                <Row className="mb-3">
                    <Col>
                        <Input type="text" name="firstName" value={resident.firstName} onChange={handleInputChange} />
                    </Col>
                    <Col>
                        <Input type="text" name="lastName" value={resident.lastName} onChange={handleInputChange} />
                    </Col>
                </Row>
                <Row>
                    <Col>
                        <Label>
                            <Input className="mx-2" type="checkbox" name="isInside" checked={resident.isInside} onChange={handleInputChange} /> Inside
                        </Label>
                    </Col>
                    <Col className="text-end">
                        <Button color="primary" onClick={handleUpdate}>Save</Button>
                        <Button color="secondary" className="ms-2" onClick={() => navigate("/residents")}>Cancel</Button>
                    </Col>
                </Row>
                
            </Form>
        </Container>
    );
};

export default EditResident;
