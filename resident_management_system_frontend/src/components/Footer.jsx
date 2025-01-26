import React from "react";
import "../styles/Footer.css";
import {Container, Row, Col, ListGroup, ListGroupItem} from 'reactstrap';
import { Link } from "react-router-dom";

const Footer = () => {

  const year = new Date().getFullYear()

  return (
    <footer className="footer">
      <Container>
        <Row>
          <h5 className="footer__link-title">Kontakt</h5>
        </Row>
        <Row>
          <Col>
            <ListGroup>
              <ListGroupItem className='ps-3 border-0 d-flex align-items-center gap-3'>
              <h6 className='mb-0 d-flex align-items-center gap-2'>
              <b> <i>Adresa:</i></b>
              
              <p className='mb-0'>Fakultetska 1, Zenica</p>
            </h6>
              </ListGroupItem>
            </ListGroup>
          </Col>
          <Col>
            <ListGroup className='footer__quick-links'>
                <ListGroupItem className='ps-3 border-0 d-flex align-items-center gap-3'>
                  <h6 className='mb-0 d-flex align-items-center gap-2'>
                    <b><i class='ri-mail-line'>Email:</i></b>
                    
                  </h6>

                  <p className='mb-0'>delic.amer.21@size.ba</p>
                </ListGroupItem>
                
            </ListGroup>
            
          </Col>
          <Col>
            <ListGroup>
              <ListGroupItem className='ps-3 border-0 d-flex align-items-center gap-3'>
                <h6 className='mb-0 d-flex align-items-center gap-2'>
                  <b><i class='ri-phone-fill'>Broj telefona:</i></b>
                </h6>
                <p className='mb-0'>+38762015130</p>
              </ListGroupItem>
            </ListGroup>
          </Col>
          <Col lg='12' className='text-center pt-5'>
            <p className="copyright">Resident Management System - Test Assignment {year}</p>
          </Col>
        </Row>
      </Container>
    </footer>
  );
}

export default Footer;
