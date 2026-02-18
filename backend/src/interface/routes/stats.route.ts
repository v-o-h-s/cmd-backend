import { Router } from "express";
import { authMiddleware } from "../middlewares/authMiddleware";
import { requireRole } from "../middlewares/requireRole";
import { Role } from "../../shared/lib/roles";
import { asyncWrapper } from "../../shared/utils/asyncWrapper";
import { statsController as controller } from "../../config/container";

const router = Router();

/**
 * @swagger
 * components:
 *   schemas:
 *     PatientsPerDayItem:
 *       type: object
 *       properties:
 *         date:
 *           type: string
 *           format: date
 *           example: "2025-12-22"
 *         count:
 *           type: integer
 *           example: 2
 *     AppointmentsPerDayItem:
 *       type: object
 *       properties:
 *         date:
 *           type: string
 *           format: date
 *           example: "2025-12-22"
 *         count:
 *           type: integer
 *           example: 3
 *     DashboardStats:
 *       type: object
 *       properties:
 *         totalPatients:
 *           type: integer
 *         activeSessions:
 *           type: integer
 *         activemachines:
 *           type: integer
 *         staffCount:
 *           type: integer
 *         staffSublabel:
 *           type: string
 *         patientsThisWeek:
 *           type: integer
 *         appointmentsThisWeek:
 *           type: integer
 *         patientsPerDay:
 *           type: array
 *           items:
 *             $ref: '#/components/schemas/PatientsPerDayItem'
 *         appointmentsPerDay:
 *           type: array
 *           items:
 *             $ref: '#/components/schemas/AppointmentsPerDayItem'
 *     StatsSummary:
 *       type: object
 *       properties:
 *         totalPatients:
 *           type: integer
 *         activeSessions:
 *           type: integer
 *         activemachines:
 *           type: integer
 *         staffCount:
 *           type: integer
 *         staffSublabel:
 *           type: string
 *         patientsThisWeek:
 *           type: integer
 *         appointmentsThisWeek:
 *           type: integer
 */

/**
 * @swagger
 * /stats:
 *   get:
 *     tags: [Stats]
 *     summary: Get consolidated dashboard statistics
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Dashboard statistics
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 status:
 *                   type: integer
 *                 data:
 *                   $ref: '#/components/schemas/DashboardStats'
 *                 error:
 *                   nullable: true
 *                   type: object
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden
 */
router.get(
  "/",
  authMiddleware,
  asyncWrapper(controller.getStats.bind(controller)),
);

/**
 * @swagger
 * /stats/patients-per-day:
 *   get:
 *     tags: [Stats]
 *     summary: Get patients created per day for the current week
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Patients per day
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 status:
 *                   type: integer
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/PatientsPerDayItem'
 *                 error:
 *                   nullable: true
 *                   type: object
 */
router.get(
  "/patients-per-day",
  authMiddleware,
  asyncWrapper(controller.getPatientsPerDay.bind(controller)),
);

/**
 * @swagger
 * /stats/appointments-per-day:
 *   get:
 *     tags: [Stats]
 *     summary: Get appointments scheduled per day for the current week
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Appointments per day
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 status:
 *                   type: integer
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/AppointmentsPerDayItem'
 *                 error:
 *                   nullable: true
 *                   type: object
 */
router.get(
  "/appointments-per-day",
  authMiddleware,
  asyncWrapper(controller.getAppointmentsPerDay.bind(controller)),
);

/**
 * @swagger
 * /stats/summary:
 *   get:
 *     tags: [Stats]
 *     summary: Get compact stats summary
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Stats summary
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 status:
 *                   type: integer
 *                 data:
 *                   $ref: '#/components/schemas/StatsSummary'
 *                 error:
 *                   nullable: true
 *                   type: object
 */
router.get(
  "/summary",
  authMiddleware,
  asyncWrapper(controller.getSummary.bind(controller)),
);

export default router;
