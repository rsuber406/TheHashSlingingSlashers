using UnityEngine;
using System.Collections;

public class WallRunController
{
    private CharacterController controller;
    private float wallRunSpeed;
    private float wallRunDuration;
    private bool isWallRunning;

    public WallRunController(CharacterController controller, float wallRunSpeed, float wallRunDuration)
    {
        this.controller = controller;
        this.wallRunSpeed = wallRunSpeed;
        this.wallRunDuration = wallRunDuration;
    }

    public void StartWallRun(Vector3 wallNormal)
    {
        if (!isWallRunning)
        {
            isWallRunning = true;

            Vector3 wallDirection = Vector3.Cross(wallNormal, Vector3.up);
            Quaternion targetRotation = Quaternion.LookRotation(wallDirection);

            // Lock the player's rotation to the wall, slerp makes the transition smoother
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.deltaTime * 10f);

            // controller.Move(wallNormal * wallRunSpeed * Time.deltaTime);
            // This is lazy, but does making this a monobehaviour make it better?
            GameManager.instance.StartCoroutine(EndWallRun_Internal());
        }
    }

    private IEnumerator EndWallRun_Internal()
    {
        yield return new WaitForSeconds(wallRunDuration);
        EndWallRun();
    }

    public void EndWallRun()
    {
        isWallRunning = false;
        Debug.Log("WallRunEnd");
    }

    public bool IsWallRunning()
    {
        return isWallRunning && !controller.isGrounded;
    }

    public void Move(Vector3 direction)
    {
        Vector3 moveDirection = new Vector3(direction.x, 0, direction.z).normalized;
        controller.Move(moveDirection * wallRunSpeed * Time.deltaTime);
    }
} 