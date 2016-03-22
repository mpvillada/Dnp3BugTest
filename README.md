# Dnp3BugTest

This is a test for a "possible" bug happening in opendnp3 library.

We set up a master and an outstation. The outstation is configured with 256 Analogs.

The master is configured with an AllEventClasses poll every 500ms.

The outstation changes 100 random analogs to random values every 1000ms.
Then we proceed to listen to all the data that comes from the DNP3 interface and check it against the data that has been changed in the outstation.
Every data change in the outstation must be notified just one time.

The "possible" bug:
When running the test, after a while, some data comes replicated in the ISOEHandler:

Example of the test log:

----------Changing value:Analog_188 -> 188;Value:9726
----------Changing value:Analog_161 -> 161;Value:4978
...
----------Changing value:Analog_235 -> 235;Value:6911
----------Changing value:Analog_115 -> 115;Value:7850
----------Changing value:Analog_230 -> 230;Value:8809
----------Changing value:Analog_115 -> 115;Value:2155
----------Changing value:Analog_119 -> 119;Value:678
----------Changing value:Analog_45 -> 45;Value:5017
...
Getting data:Analog_131 value:1693
Getting data:Analog_235 value:6911
Getting data:Analog_115 value:7850
Getting data:Analog_230 value:8809
Getting data:Analog_115 value:2155
Getting data:Analog_119 value:678
Getting data:Analog_45 value:5017
...
----------Changing value:Analog_150 -> 150;Value:4370
----------Changing value:Analog_115 -> 115;Value:3512
...
Getting data:Analog_115 value:3512
...
Getting data:Analog_115 value:3512
----------Changing value:Analog_109 -> 109;Value:6716
ERROR with tag:Analog_115 value:3512

As can be seen, the data "Analog_115" goes through the values 7850, 2155, 3512. Every value is notified in
the ISOEHandler, but 3512 is notified twice.
