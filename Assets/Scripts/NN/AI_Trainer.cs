﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;

public class AI_Trainer : MonoBehaviour
{
	public Population population;
	NeuralNetwork.NeuralNetwork currNN;

	AI_DriveController driveController;
	Transform raycastPoint;
	RaycastHit[] sensors;
	GameObject environments;
	GameObject[] lines;
	public Material lineRendererMaterial;

	Vector3 startPosition;
	Quaternion startRotation;

	Vector3 currCarPos;
	Vector3 lastCarPos;
	public float totalDist;
	public float timePassed;

	public float timeScale = 1f;

	float rayDist = 11;

	// Use this for initialization
	void Start()
	{
		population = new Population(10, new int[] { 5, 200, 2 }, 1f);

		raycastPoint = transform.Find("RaycastPoint");
		environments = GameObject.Find("Environment");
		driveController = GetComponent<AI_DriveController>();

		startPosition = transform.position;
		startRotation = transform.rotation;

		currCarPos = lastCarPos = startPosition;

		currNN = population.Next();
	}

	public void NewGenome()
	{
		OnCollisionEnter();
	}

	public void ChangeSpeed()
	{
		if (timeScale == 1f)
			timeScale = 2f;
		else if (timeScale == 2f)
			timeScale = 5f;
		else if (timeScale == 5f)
			timeScale = 10f;
		else if (timeScale == 10f)
			timeScale = 1f;
	}

	// Update is called once per frame
	void Update()
	{
		Time.timeScale = timeScale;

		sensors = new RaycastHit[5];

		Physics.Raycast(raycastPoint.position, raycastPoint.forward, out sensors[0], rayDist);
		Physics.Raycast(raycastPoint.position, raycastPoint.forward - raycastPoint.right, out sensors[3], rayDist);
		Physics.Raycast(raycastPoint.position, raycastPoint.forward + raycastPoint.right, out sensors[4], rayDist);
		Physics.Raycast(raycastPoint.position, -raycastPoint.right, out sensors[1], rayDist);
		Physics.Raycast(raycastPoint.position, raycastPoint.right, out sensors[2], rayDist);

		DrawSensorLines();

		float forward, left, right, leftMid, rightMid;
		forward = left = right = leftMid = rightMid = rayDist;

		if (sensors[0].collider != null)
			forward = sensors[0].distance;

		if (sensors[1].collider != null)
			left = sensors[1].distance;

		if (sensors[2].collider != null)
			right = sensors[2].distance;

		if (sensors[3].collider != null)
			leftMid = sensors[3].distance;

		if (sensors[4].collider != null)
			rightMid = sensors[4].distance;

		NeuralNetwork.Matrix inputs = new NeuralNetwork.Matrix(5, 1);
		inputs.matrix[0,0] = (2f / rayDist) * forward - 1f;
		inputs.matrix[1,0] = (2f / rayDist) * left - 1f;
		inputs.matrix[2,0] = (2f / rayDist) * right - 1f;
		inputs.matrix[3,0] = (2f / rayDist) * leftMid - 1f;
		inputs.matrix[4,0] = (2f / rayDist) * rightMid - 1f;

		
		currNN.input = inputs;
		currNN.FeedForward();

		driveController.SetMaxSpeed(currNN.output.matrix[0,0]);

		currCarPos = transform.position;
		totalDist += Vector3.Distance(currCarPos, lastCarPos);
		lastCarPos = currCarPos;

		timePassed += Time.deltaTime;
	}

	void OnCollisionEnter()
	{
		population.SetFitnessOfCurrIndividual(totalDist);
		currNN = population.Next();
		ResetCarPosition();
	}

	void ResetCarPosition()
	{
		transform.position = startPosition;
		transform.rotation = startRotation;
		currCarPos = startPosition;
		lastCarPos = startPosition;

		driveController.SetMotorTorque(0f);
		driveController.GetComponent<Rigidbody>().velocity = Vector3.zero;
		totalDist = 0f;
		timePassed = 0f;
	}

	void DrawSensorLines()
	{
		Color middleSensorColor, leftSensorColor, rightSensorColor, leftMiddleSensorColor, rightMiddleSensorColor;
		middleSensorColor = (sensors[0].collider == null) ? Color.green : Color.red;
		leftSensorColor = (sensors[1].collider == null) ? Color.green : Color.red;
		rightSensorColor = (sensors[2].collider == null) ? Color.green : Color.red;
		leftMiddleSensorColor = (sensors[3].collider == null) ? Color.green : Color.red;
		rightMiddleSensorColor = (sensors[4].collider == null) ? Color.green : Color.red;

		if (lines == null)
		{
			lines = new GameObject[5];
			DrawLine(
				raycastPoint.position,
				(raycastPoint.position + raycastPoint.forward * rayDist),
				middleSensorColor,
				0
			);

			DrawLine(
				raycastPoint.position,
				(raycastPoint.position + (raycastPoint.forward - raycastPoint.right) * rayDist),
				leftMiddleSensorColor,
				1
			);

			DrawLine(
				raycastPoint.position,
				(raycastPoint.position + (raycastPoint.forward + raycastPoint.right) * rayDist),
				rightMiddleSensorColor,
				2
			);

			DrawLine(
				raycastPoint.position,
				(raycastPoint.position + (-raycastPoint.right) * rayDist),
				leftMiddleSensorColor,
				3
			);

			DrawLine(
				raycastPoint.position,
				(raycastPoint.position + raycastPoint.right * rayDist),
				rightMiddleSensorColor,
				4
			);
		}
		else
		{
			UpdateLine(
				raycastPoint.position,
				(raycastPoint.position + raycastPoint.forward * rayDist),
				middleSensorColor,
				0
			);

			UpdateLine(
				raycastPoint.position,
				(raycastPoint.position + (raycastPoint.forward - raycastPoint.right).normalized * rayDist),
				leftMiddleSensorColor,
				1
			);

			UpdateLine(
				raycastPoint.position,
				(raycastPoint.position + (raycastPoint.forward + raycastPoint.right).normalized * rayDist),
				rightMiddleSensorColor,
				2
			);

			UpdateLine(
				raycastPoint.position,
				(raycastPoint.position + (-raycastPoint.right).normalized * rayDist),
				leftSensorColor,
				3
			);

			UpdateLine(
				raycastPoint.position,
				(raycastPoint.position + raycastPoint.right.normalized * rayDist),
				rightSensorColor,
				4
			);
		}

	}

	void DrawLine(Vector3 start, Vector3 end, Color color, int lineIndex)
	{
		GameObject line = new GameObject();
		line.name = "Line " + lineIndex;
		line.transform.SetParent(environments.transform);

		line.transform.position = start;
		line.AddComponent<LineRenderer>();
		LineRenderer lr = line.GetComponent<LineRenderer>();
		lr.material = lineRendererMaterial; //new Material(Shader.Find("Particles/Priority Alpha Blended"));
		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);

		lines[lineIndex] = line;
	}

	void UpdateLine(Vector3 start, Vector3 end, Color color, int lineIndex)
	{
		LineRenderer lr = lines[lineIndex].GetComponent<LineRenderer>();
		lr.startColor = color;
		lr.endColor = color;
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
	}
}
