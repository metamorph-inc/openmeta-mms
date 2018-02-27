.. _ara_test_bench_basics:

Project Ara Test Bench Basics
=============================

Configuring
~~~~~~~~~~~

If you don't already have a *test bench* folder in your project, create
one:

1. In the **GME Browser**, right-click on the **RootFolder**.
2. Select **Insert Folder --> Testing**.
3. The default name of this folder is **Testing**, but you may rename
   it if you like.

Next, we must create a copy of the *test bench* we want to use.

1. In the **GME Browser**, expand the purple folder labeled
   **TestBenches**.
2. Expand the folder labeled **AraTestBenches**.
3. Right-click on the *test bench* you want to use, and select **Copy**.
4. Right-click on your *test bench folder* and select **Paste**.

Next, we must place your design or design space into the test bench.

1. Open your copy of the test bench.
2. Right-click on your design, drag it onto the canvas, and select
   **Paste as Reference**.
3. Choose **TopLevelSystemUnderTest** and click OK.

Next, you will have to do some configuration specific to the *test
bench* that you're using. For these details and more, refer to the
document for the *test bench* that you're using.

Running a Test Bench
~~~~~~~~~~~~~~~~~~~~

To run a test bench:

1. Open the test bench so that it appears in the editing window.
2. Click the **Master Interpreter** on the **GME Toolbar**:
   |MasterInterpreter Icon|
3. Select **Post to META Job Manager** to have the analysis run
   automatically.
4. If you're testing a *design space*, select the *configurations* you'd
   like to test in the list at the right.
5. Click **OK**.
6. The path to the generated artifacts will be shown on the GME Console,
   with the message ``Generated files are here: <path>``

If you selected **Post to META Job Manager**, then the **Job Manager**
will launch, and you'll see test benches begin executing in the **Job
Manager** window.


..  Viewing Metrics
    ~~~~~~~~~~~~~~~

    Some *test benches* produce metrics, which are values which can help you
    compare designs. To view the metrics that have been gathered for your
    designs, launch the *Project Analyzer* by opening the ``index.html``
    file in your project's root directory. Be sure that **Google Chrome**
    has been configured as described in `Chapter 2: Installation and
    Setup <@ref%20configure-chrome-to-run-the-project-analyzer>`__.

    For more information on using the *Project Analyzer*, see `Project
    Analyzer <@ref%20project-analyzer>`__.

.. |MasterInterpreter Icon| image:: images/11-00-master-interpreter-icon.png
