import { Router } from "express";
import { authMiddleware } from "../middlewares/authMiddleware";
import { requireRole } from "../middlewares/requireRole";
import { Role } from "../../shared/lib/roles";
import { validate } from "../middlewares/Validate";
import { createMachineSchemaDto } from "../../application/dto/requests/createMachineDto";
import { updateMachineSchemaDto } from "../../application/dto/requests/updateMachineDto";
import { asyncWrapper } from "../../shared/utils/asyncWrapper";
import { machineController } from "../../config/container";
import { ResponseFormatter } from "../utils/ResponseFormatter";

const router = Router();

/**
/**
 * @swagger
 * components:
 *   schemas:
 *     Machine:
 *       type: object
 *       properties:
 *         id:
 *           type: string
 *           format: uuid
 *         machineId:
 *           type: string
 *         manufacturer:
 *           type: string
 *         model:
 *           type: string
 *         status:
 *           type: string
 *           enum: [available, in-use, maintenance, out-of-service]
 *         lastMaintenanceDate:
 *           type: string
 *           format: date
 *         nextMaintenanceDate:
 *           type: string
 *           format: date
 *         isActive:
 *           type: boolean
 *         roomId:
 *           type: string
 *           format: uuid
 *         createdAt:
 *           type: string
 *           format: date-time
 *         updatedAt:
 *           type: string
 *           format: date-time
 *
 *     CreateMachineDto:
 *       type: object
 *       properties:
 *         machineId:
 *           type: string
 *         manufacturer:
 *           type: string
 *         model:
 *           type: string
 *         status:
 *           type: string
 *           enum: [available, in-use, maintenance, out-of-service]
 *         lastMaintenanceDate:
 *           type: string
 *           format: date
 *         nextMaintenanceDate:
 *           type: string
 *           format: date
 *         roomId:
 *           type: string
 *           format: uuid
 *         isActive:
 *           type: boolean
 *       required:
 *         - lastMaintenanceDate
 *         - nextMaintenanceDate
 *
 *     UpdateMachineDto:
 *       allOf:
 *         - $ref: '#/components/schemas/CreateMachineDto'
 *       description: Partial fields allowed for update (all optional)
 *     MachineStatsFormatted:
 *       type: object
 *       properties:
 *         In_Use:
 *           type: integer
 *         Available:
 *           type: integer
 *         Out_of_Service:
 *           type: integer
 *         Maintenance:
 *           type: integer
 *         total:
 *           type: integer
 *
 *     MachineResponse:
 *       type: object
 *       properties:
 *         success:
 *           type: boolean
 *         status:
 *           type: integer
 *         data:
 *           $ref: '#/components/schemas/Machine'
 *         error:
 *           nullable: true
 *           type: object
 */

/**
 * @swagger
 * /machines:
 *   post:
 *     tags: [Machines]
 *     summary: Create a new machine
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/CreateMachineDto'
 *     responses:
 *       201:
 *         description: Machine created successfully
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/MachineResponse'
 *       400:
 *         description: Validation error
 */
router.post(
  "/",
  authMiddleware,
  asyncWrapper(machineController.createMachine.bind(machineController)),
);

/**
 * @swagger
 * /machines:
 *   get:
 *     tags: [Machines]
 *     summary: Retrieve list of machines
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: query
 *         name: status
 *         schema:
 *           type: string
 *           enum: [available, in-use, maintenance, out-of-service]
 *         description: Filter by machine status
 *       - in: query
 *         name: room
 *         schema:
 *           type: string
 *         description: Filter by room
 *     responses:
 *       200:
 *         description: List of machines
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
 *                     $ref: '#/components/schemas/Machine'
 *                 error:
 *                   nullable: true
 */
router.get(
  "/",
  authMiddleware,
  requireRole([Role.ADMIN, Role.DOCTOR, Role.RECEPTIONIST]),
  asyncWrapper(machineController.getMachines.bind(machineController)),
);

/**
 * @swagger
 * /machines/stats:
 *   get:
 *     tags: [Machines]
 *     summary: Get overall machine statistics
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Machine statistics retrieved
 */
router.get(
  "/stats",
  authMiddleware,
  requireRole([Role.ADMIN, Role.DOCTOR, Role.RECEPTIONIST]),
  asyncWrapper(machineController.getStats.bind(machineController)),
);

/**
 * @swagger
 * /machines/machine-stats:
 *   get:
 *     tags: [Machines]
 *     summary: Returns machine statistics with formatted keys
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Machine statistics (formatted)
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                 status:
 *                   type: number
 *                 data:
 *                   $ref: '#/components/schemas/MachineStatsFormatted'
 *                 error:
 *                   nullable: true
 *                   type: object
 */
router.get(
  "/machine-stats",
  authMiddleware,
  requireRole([Role.ADMIN, Role.DOCTOR, Role.RECEPTIONIST]),
  asyncWrapper(machineController.getFormattedStats.bind(machineController)),
);

/**
 * @swagger
 * /machines/{id}:
 *   get:
 *     tags: [Machines]
 *     summary: Get machine by id
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Machine found
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/MachineResponse'
 *       404:
 *         description: Machine not found
 */
router.get(
  "/:id",
  authMiddleware,
  requireRole([Role.ADMIN, Role.DOCTOR, Role.RECEPTIONIST]),
  asyncWrapper(machineController.getMachineById.bind(machineController)),
);

/**
 * @swagger
 * /machines/{id}:
 *   put:
 *     tags: [Machines]
 *     summary: Update a machine
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/UpdateMachineDto'
 *     responses:
 *       200:
 *         description: Machine updated successfully
 */
router.put(
  "/:id",
  authMiddleware,
  requireRole([Role.ADMIN]),
  validate(updateMachineSchemaDto),
  asyncWrapper(machineController.updateMachine.bind(machineController)),
);

/**
 * @swagger
 * /machines/{id}/deactivate:
 *   patch:
 *     tags: [Machines]
 *     summary: Deactivate a machine (mark as out-of-service)
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *           format: uuid
 *     responses:
 *       200:
 *         description: Machine deactivated successfully
 */
router.patch(
  "/:id/deactivate",
  authMiddleware,
  requireRole([Role.ADMIN]),
  asyncWrapper(machineController.deactivateMachine.bind(machineController)),
);

export default router;
