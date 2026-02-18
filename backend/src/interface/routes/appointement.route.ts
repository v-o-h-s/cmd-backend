import { Router } from "express";
import { appointementController } from "../../config/container";
import { authMiddleware } from "../middlewares/authMiddleware";
import { asyncWrapper } from "../../shared/utils/asyncWrapper";
import { requireRole } from "../middlewares/requireRole";
import { validate } from "../middlewares/Validate";
import { addAppointmentDto } from "../../application/dto/requests/addAppointementDto";
import { createAppointmentHistoryDto } from "../../application/dto/requests/appointmentHistory/createAppointmentHistoryDto";
import { updateAppointmentHistoryDto } from "../../application/dto/requests/appointmentHistory/updateAppointmentHistoryDto";
import { completeAppointmentDto } from "../../application/dto/requests/appointmentHistory/completeAppointmentDto";
import { Role } from "../../shared/lib/roles";

const router = Router();

/**
 * @swagger
 * components:
 *   schemas:
 *     Appointment:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           format: uuid
 *           description: The appointment ID
 *         patientId:
 *           type: string
 *           format: uuid
 *           description: The patient ID
 *         doctorId:
 *           type: string
 *           format: uuid
 *           description: The doctor ID
 *         appointmentDate:
 *           type: string
 *           format: date-time
 *           description: Appointment date and time
 *         reason:
 *           type: string
 *           description: Reason for the appointment
 *         status:
 *           type: string
 *           description: Current status of the appointment
 *         notes:
 *           type: string
 *           nullable: true
 *           description: Additional notes
 *         roomId:
 *           type: string
 *           format: uuid
 *           nullable: true
 *           description: The room ID
 *         createdAt:
 *           type: string
 *           format: date-time
 *         updatedAt:
 *           type: string
 *           format: date-time
 *
 *     CreateAppointmentRequest:
 *       type: object
 *       required:
 *         - patientId
 *         - doctorId
 *         - appointmentDate
 *         - reason
 *       properties:
 *         patientId:
 *           type: string
 *           format: uuid
 *           example: 123e4567-e89b-12d3-a456-426614174000
 *         doctorId:
 *           type: string
 *           format: uuid
 *           example: 987fcdeb-51a2-43f1-b9c8-123456789abc
 *         appointmentDate:
 *           type: string
 *           format: date-time
 *           example: 2024-12-15T14:30:00.000Z
 *         reason:
 *           type: string
 *           example: Regular checkup
 *         status:
 *           type: string
 *           example: SCHEDULED
 *         notes:
 *           type: string
 *           nullable: true
 *           example: Patient requested afternoon slot
 *         roomId:
 *           type: string
 *           format: uuid
 *           nullable: true
 *
 *     AppointmentHistory:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           format: uuid
 *           description: The history record ID
 *         appointmentId:
 *           type: string
 *           format: uuid
 *           description: The appointment ID
 *         appointmentData:
 *           type: object
 *           description: Appointment results/data snapshot
 *         createdAt:
 *           type: string
 *           format: date-time
 *           description: When the history was recorded
 *         updatedAt:
 *           type: string
 *           format: date-time
 *           description: When the history was last updated
 *
 *     UpdateAppointmentHistoryRequest:
 *       type: object
 *       properties:
 *         appointmentData:
 *           type: object
 *           description: Updated appointment data
 *
 *     AppointmentDetail:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           format: uuid
 *         patient:
 *           type: object
 *           nullable: true
 *           properties:
 *             id:
 *               type: string
 *               format: uuid
 *             first_name:
 *               type: string
 *             last_name:
 *               type: string
 *         doctor:
 *           type: object
 *           nullable: true
 *           properties:
 *             id:
 *               type: string
 *               format: uuid
 *             first_name:
 *               type: string
 *             last_name:
 *               type: string
 *         roomId:
 *           type: string
 *           format: uuid
 *           nullable: true
 *         createdByReceptionistId:
 *           type: string
 *           format: uuid
 *           nullable: true
 *         createdByDoctorId:
 *           type: string
 *           format: uuid
 *           nullable: true
 *         appointmentDate:
 *           type: string
 *           format: date-time
 *         estimatedDurationInMinutes:
 *           type: number
 *         status:
 *           type: string
 */

/**
 * @swagger
 * tags:
 *   name: Appointments
 *   description: The appointments managing API
 */

/**
 * @swagger
 * /appointments:
 *   post:
 *     summary: Create an appointment
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/CreateAppointmentRequest'
 *     responses:
 *       201:
 *         description: Appointment created successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                   example: Appointment created successfully
 *                 data:
 *                   $ref: '#/components/schemas/AppointmentDetail'
 *                 error:
 *                   type: null
 *       400:
 *         description: Validation error
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - Receptionist, Doctor or Admin role required
 *       404:
 *         description: Room not found
 *       409:
 *         description: Conflict - room is already booked or not available for the requested time
 */
router.post(
  "/",
  authMiddleware,
  requireRole([Role.RECEPTIONIST, Role.DOCTOR, Role.ADMIN]),
  validate(addAppointmentDto),
  asyncWrapper(
    appointementController.createAppointment.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments:
 *   get:
 *     summary: Get all appointments with optional time-based and name-based filtering
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: view
 *         schema:
 *           type: string
 *           enum: [year, month, week, day, all]
 *           default: month
 *         description: Time period filter
 *       - in: query
 *         name: date
 *         schema:
 *           type: string
 *           format: date
 *         description: Reference date for filtering (ISO 8601 format)
 *       - in: query
 *         name: patientName
 *         schema:
 *           type: string
 *         description: Filter by patient name (partial match)
 *       - in: query
 *         name: doctorName
 *         schema:
 *           type: string
 *         description: Filter by doctor name (partial match)
 *     responses:
 *       200:
 *         description: List of appointments
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                   example: Appointments retrieved successfully
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/Appointment'
 *                 error:
 *                   type: null
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 */
// Get all appointments with optional view filter (year/month/week/day) and optional name filters
router.get(
  "/",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.RECEPTIONIST, Role.ADMIN]),
  asyncWrapper(
    appointementController.getAllAppointments.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/{appointmentId}:
 *   get:
 *     summary: Get an appointment by ID
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: appointmentId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The appointment ID
 *     responses:
 *       200:
 *         description: Appointment retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                   example: Appointment retrieved successfully
 *                 data:
 *                   $ref: '#/components/schemas/Appointment'
 *                 error:
 *                   type: null
 *       404:
 *         description: Appointment not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 */
// Get appointment by ID
router.get(
  "/:appointmentId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.RECEPTIONIST, Role.ADMIN]),
  asyncWrapper(
    appointementController.getAppointmentById.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/doctor/{doctorId}:
 *   get:
 *     summary: Get appointments by doctor ID
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: doctorId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The doctor ID
 *       - in: query
 *         name: view
 *         schema:
 *           type: string
 *           enum: [year, month, week, day, all]
 *           default: month
 *         description: Time period filter
 *       - in: query
 *         name: date
 *         schema:
 *           type: string
 *           format: date
 *         description: Reference date for filtering
 *     responses:
 *       200:
 *         description: Doctor's appointments retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/Appointment'
 *                 error:
 *                   type: null
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       404:
 *         description: Doctor not found
 */
// Get appointments by doctor ID
router.get(
  "/doctor/:doctorId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.RECEPTIONIST, Role.ADMIN]),
  asyncWrapper(
    appointementController.getAppointmentsByDoctor.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/patient/{patientId}:
 *   get:
 *     summary: Get appointments by patient ID
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: patientId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The patient ID
 *       - in: query
 *         name: view
 *         schema:
 *           type: string
 *           enum: [year, month, week, day, all]
 *           default: month
 *         description: Time period filter
 *       - in: query
 *         name: date
 *         schema:
 *           type: string
 *           format: date
 *         description: Reference date for filtering
 *     responses:
 *       200:
 *         description: Patient's appointments retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/Appointment'
 *                 error:
 *                   type: null
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       404:
 *         description: Patient not found
 */
// Get appointments by patient ID
router.get(
  "/patient/:patientId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.RECEPTIONIST, Role.ADMIN]),
  asyncWrapper(
    appointementController.getAppointmentsByPatient.bind(
      appointementController,
    ),
  ),
);

/**
 * @swagger
 * /appointments/{appointmentId}:
 *   delete:
 *     summary: Delete/cancel an appointment
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: appointmentId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The appointment ID
 *     responses:
 *       200:
 *         description: Appointment deleted successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                   example: Appointment deleted successfully
 *                 data:
 *                   type: null
 *                 error:
 *                   type: null
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 *       404:
 *         description: Appointment not found
 */
// Delete appointment
router.delete(
  "/:appointmentId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.RECEPTIONIST, Role.ADMIN]),
  asyncWrapper(
    appointementController.deleteAppointment.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/{appointmentId}/complete:
 *   post:
 *     summary: Complete an appointment and record medical history
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: appointmentId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The appointment ID
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - patientId
 *               - doctorId
 *             properties:
 *               patientId:
 *                 type: string
 *                 format: uuid
 *                 description: The patient ID
 *               doctorId:
 *                 type: string
 *                 format: uuid
 *                 description: The doctor ID
 *               appointmentData:
 *                 type: object
 *                 description: Medical data/results from appointment
 *     responses:
 *       200:
 *         description: Appointment completed and history recorded successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 data:
 *                   type: null
 *                 error:
 *                   type: null
 *       400:
 *         description: Bad request or validation error
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - Doctor or Admin only
 */

// update the medical file and also the appointment history after a appointment is completed
router.post(
  "/:appointmentId/complete",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.ADMIN]),
  validate(completeAppointmentDto),
  asyncWrapper(
    appointementController.completeAppointment.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/history/{appointmentId}:
 *   get:
 *     summary: Get result/history for an appointment
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: appointmentId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The appointment ID
 *     responses:
 *       200:
 *         description: Appointment history retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 data:
 *                   $ref: '#/components/schemas/AppointmentHistory'
 *                 error:
 *                   type: null
 *       404:
 *         description: Appointment history not found
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/history/:appointmentId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.ADMIN]),

  asyncWrapper(
    appointementController.getHistoryByAppointmentId.bind(
      appointementController,
    ),
  ),
);

/**
 * @swagger
 * /appointments/history/patient/{patientId}:
 *   get:
 *     summary: Get all appointment results/history for a patient
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: patientId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The patient ID
 *     responses:
 *       200:
 *         description: Appointment histories retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/AppointmentHistory'
 *                 error:
 *                   type: null
 *       401:
 *         description: Unauthorized
 */
router.get(
  "/history/patient/:patientId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.ADMIN]),

  asyncWrapper(
    appointementController.getHistoriesByPatientId.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/history/{appointmentId}:
 *   patch:
 *     summary: Update appointment results/history data
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: appointmentId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The appointment ID
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/UpdateAppointmentHistoryRequest'
 *     responses:
 *       200:
 *         description: Appointment history updated successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 data:
 *                   $ref: '#/components/schemas/AppointmentHistory'
 *                 error:
 *                   type: null
 *       400:
 *         description: Bad request
 *       401:
 *         description: Unauthorized
 */
router.patch(
  "/history/:appointmentId",
  authMiddleware,
  requireRole([Role.DOCTOR, Role.ADMIN]),
  validate(updateAppointmentHistoryDto),
  asyncWrapper(
    appointementController.updateHistory.bind(appointementController),
  ),
);

/**
 * @swagger
 * /appointments/history/{appointmentId}:
 *   delete:
 *     summary: Delete appointment results/history record
 *     tags: [Appointments]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: appointmentId
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *         description: The appointment ID
 *     responses:
 *       200:
 *         description: Appointment history deleted successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 message:
 *                   type: string
 *                 data:
 *                   type: null
 *                 error:
 *                   type: null
 *       404:
 *         description: Appointment history not found
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - admin only
 */
router.delete(
  "/history/:appointmentId",
  authMiddleware,
  requireRole([Role.ADMIN]),
  asyncWrapper(
    appointementController.deleteHistory.bind(appointementController),
  ),
);

export default router;
