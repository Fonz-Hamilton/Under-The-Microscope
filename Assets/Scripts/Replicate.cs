using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Replicate : MonoBehaviour {
    public Player player;
    public GameObject clonePrefab; // Reference to the clone prefab
    public float interval = 2f;


    void Start() {
        StartCoroutine(ReplicateBacteria());
    }

    private IEnumerator ReplicateBacteria() {
        while (true) {
            if (player.currentEnergy > 0) {
                new WaitForSeconds(interval);
                ReplicatePlayer();

                // Wait for the replication interval
                yield return new WaitForSeconds(interval);
            }
            else {
                // If the energy level is 0 or below, wait for a short time before checking again
                yield return new WaitForSeconds(1f);
            }
        }
    }
    private void ReplicatePlayer() {
        // Instantiate a new player GameObject at the same position as the current player
        GameObject newPlayer = Instantiate(clonePrefab, transform.position, Quaternion.identity); ;

    }

}
