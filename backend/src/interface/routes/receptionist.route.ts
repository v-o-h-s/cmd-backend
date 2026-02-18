import { Router } from "express";
import { authMiddleware } from "../middlewares/authMiddleware";
import { asyncWrapper } from "../../shared/utils/asyncWrapper";
import { requireRole } from "../middlewares/requireRole";
import { receptionistController as receptionistsController } from "../../config/container";

const router = Router();

/**
 * @swagger
 * tags:
 *   name: Receptionists
 *   description: The receptionists managing API
 */

/**
 * @swagger
 * /receptionists:
 *   get:
 *     summary: Retrieve a list of all receptionists
 *     tags: [Receptionists]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: A list of receptionists
 *       401:
 *         description: Unauthorized
 *       403:
 *         description: Forbidden - Admin only
 */
router.get(
  "/",
  authMiddleware,
  requireRole(["admin"]),
  asyncWrapper(
    receptionistsController.getReceptionists.bind(receptionistsController),
  ),
);

/**
 * @swagger
 * /receptionists/{id}:
 *   get:
 *     summary: Get a receptionist by ID
 *     tags: [Receptionists]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: The receptionist ID
 *     responses:
 *       200:
 *         description: Receptionist found
 *       404:
 *         description: Receptionist not found
 */
router.get(
  "/:id",
  authMiddleware,
  requireRole(["admin"]),
  asyncWrapper(
    receptionistsController.getReceptionistById.bind(receptionistsController),
  ),
);

/**
 * @swagger
 * /receptionists/{id}:
 *   put:
 *     summary: Update a receptionist by ID
 *     tags: [Receptionists]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               firstName:
 *                 type: string
 *               lastName:
 *                 type: string
 *               email:
 *                 type: string
 *     responses:
 *       200:
 *         description: Receptionist updated successfully
 *       404:
 *         description: Receptionist not found
 */
router.put(
  "/:id",
  authMiddleware,
  requireRole(["admin"]),
  asyncWrapper(
    receptionistsController.updateReceptionistById.bind(
      receptionistsController,
    ),
  ),
);

/**
 * @swagger
 * /receptionists/{id}:
 *   delete:
 *     summary: Delete a receptionist by ID
 *     tags: [Receptionists]
 *     security:
 *       - bearerAuth: []
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *     responses:
 *       200:
 *         description: Receptionist deleted successfully
 *       404:
 *         description: Receptionist not found
 */
router.delete(
  "/:id",
  authMiddleware,
  requireRole(["admin"]),
  asyncWrapper(
    receptionistsController.deleteReceptionistById.bind(
      receptionistsController,
    ),
  ),
);

export default router;
